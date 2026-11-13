using Animal_Spa.Models;
using Animal_Spa.Services;

// Portable smoke tests for business logic. Run with `dotnet run --project Tests/WorkflowSmoke`.
string file = Path.Combine(Path.GetTempPath(), "animal-spa-workflow-" + Guid.NewGuid().ToString("N") + ".json");
int passed = 0;

void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed++;
    Console.WriteLine("PASS " + name);
}

void MustReject(Action action, string expected, string name)
{
    try { action(); }
    catch (BookingWorkflowException ex)
    {
        Check(ex.Message.Contains(expected, StringComparison.OrdinalIgnoreCase), name);
        return;
    }
    throw new Exception("FAIL: " + name + " (operation was unexpectedly accepted)");
}

try
{
    var db = new BookingWorkflowStore(file);
    Check(db.GetBookings().Count == 6, "seed booking list");
    Check(db.GetBooking(101).Status == BookingStatuses.Pending, "PENDING status");
    Check(db.GetBooking(102).Status == BookingStatuses.Confirmed, "CONFIRMED status");
    Check(db.GetSession(103) != null, "existing session in sample");

    db.Confirm(101);
    Check(db.GetBooking(101).Status == BookingStatuses.Confirmed, "confirm pending");
    MustReject(() => db.Confirm(101), "PENDING", "block double confirm");
    db.Cancel(101);
    Check(db.GetBooking(101).Status == BookingStatuses.Cancelled, "cancel confirmed without session");
    MustReject(() => db.Confirm(101), "PENDING", "block confirm cancelled");
    MustReject(() => db.Cancel(104), "Không thể hủy", "block double cancel");
    MustReject(() => db.Cancel(105), "Không thể hủy", "block cancel completed");
    MustReject(() => db.CreateSession(101, 2, 2), "CONFIRMED", "reject session for cancelled booking");

    var availability = db.GetAvailability(102);
    Check(availability.Rooms.Count == 2 && availability.Rooms.All(r => r.Id != 1), "room 1 reserved in overlapping window");
    Check(availability.StaffMembers.Count == 2 && availability.StaffMembers.All(s => s.Id != 1), "staff A reserved in overlapping window");
    MustReject(() => db.CreateSession(102, 1, 2), "Xung đột phòng", "room collision gives readable error");
    MustReject(() => db.CreateSession(102, 2, 1), "Xung đột nhân viên", "staff collision gives readable error");
    var session = db.CreateSession(102, 2, 2);
    Check(session.BookingId == 102 && session.RoomId == 2 && session.StaffId == 2, "create session for confirmed booking");
    Check(db.GetBooking(102).Status == BookingStatuses.Confirmed, "creating session does not imply check-in");
    MustReject(() => db.CreateSession(102, 3, 3), "đã có service session", "block duplicate active session");
    MustReject(() => db.Cancel(102), "đã có service session", "block cancellation after assignment");
    var reopened = new BookingWorkflowStore(file);
    Check(reopened.GetSession(102)?.RoomId == 2, "session persisted after restart");
    Check(BookingWorkflowStore.Overlaps(
        new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 11, 20, 11, 0, 0, TimeSpan.Zero)) == false,
        "adjacent time windows do not overlap");

    var future = DateTimeOffset.Now.AddDays(4);
    var added = reopened.AddBooking(7, "Khách mới", "Bé Mây", future,
        new[] { new BookingLine { ServiceName = "Tắm", DurationMinutes = 30, UnitPrice = 150000 } }, "Test");
    Check(added.Status == BookingStatuses.Pending && reopened.GetBooking(added.Id).PetName == "Bé Mây", "new bookings start PENDING");
    Console.WriteLine($"ALL {passed} WORKFLOW TESTS PASSED");
}
finally
{
    if (File.Exists(file)) File.Delete(file);
    if (File.Exists(file + ".tmp")) File.Delete(file + ".tmp");
}
