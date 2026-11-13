"""Static dashboard checks that run without the Windows-only WPF SDK.
Run: python Tests/DashboardStaticCheck.py
Does not replace `dotnet build` / actual UI tests on Windows.
"""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
XAML_NS = '{http://schemas.microsoft.com/winfx/2006/xaml}'

def check_view(role, expected_sections, expected_buttons):
    xaml = ROOT / 'Views' / role / f'{role}Dashboard.xaml'
    cs = xaml.with_suffix('.xaml.cs')
    tree = ET.parse(xaml)
    source = cs.read_text(encoding='utf-8')
    elements = list(tree.iter())
    named = {node.attrib[XAML_NS + 'Name']: node for node in elements if XAML_NS + 'Name' in node.attrib}
    assert all(section in named for section in expected_sections), f'{role}: missing sections'
    assert all(button in named for button in expected_buttons), f'{role}: missing nav buttons'
    assert sum(named[section].get('Visibility', 'Visible') == 'Visible' for section in expected_sections) == 1
    handlers = set(re.findall(r'\b(?:Click|Loaded|SelectionChanged)="([A-Za-z_]\w*)"', xaml.read_text(encoding='utf-8')))
    assert all(re.search(r'\b' + h + r'\s*\(', source) for h in handlers), f'{role}: missing click handler'
    assert 'Logout_Click' in handlers
    return xaml.read_text(encoding='utf-8')

r = check_view('Receptionist',
               ['HomeSection', 'CustomersSection','PetsSection','ServicesSection','BookingsSection','ResourcesSection','ReportsSection'],
               ['HomeNavButton','CustomersNavButton','PetsNavButton','ServicesNavButton','BookingsNavButton','ResourcesNavButton','ReportsNavButton'])
c = check_view('Customer',
               ['HomeSection','ProfileSection','CatalogSection','AppointmentsSection','PaymentsSection'],
               ['HomeNavButton','ProfileNavButton','CatalogNavButton','AppointmentsNavButton','PaymentsNavButton'])
s = check_view('Staff',
               ['HomeSection','SessionsSection','ScheduleSection','StaffProfileSection'],
               ['HomeNavButton','SessionsNavButton','ScheduleNavButton','StaffProfileNavButton'])

assert '<components:BookingView' in r and '<components:RoomStaffView' in r
assert '<components:BookingView' not in c and '<components:RoomStaffView' not in c
assert '<components:BookingView' not in s and '<components:RoomStaffView' not in s
assert '<components:CustomerDetailView' in c and '<components:CategoryView' in c
assert '<components:' not in s, 'Staff should have no fake operational component'
assert 'ServiceDetail.SetCustomerBrowseMode()' in (ROOT/'Views/Customer/CustomerDashboard.xaml.cs').read_text(encoding='utf-8')
assert 'ShowRoomStaff(created.Id)' in (ROOT/'Views/Receptionist/ReceptionistDashboard.xaml.cs').read_text(encoding='utf-8')
assert 'Booking.SaveRequested += CreateBookingFromCart;' in (ROOT/'Views/Receptionist/ReceptionistDashboard.xaml.cs').read_text(encoding='utf-8')
assert 'RoomStaff.Confirmed += FinishSession;' in (ROOT/'Views/Receptionist/ReceptionistDashboard.xaml.cs').read_text(encoding='utf-8')

files=list(ROOT.rglob('*.xaml'))
for path in files:
    ET.parse(path)
print(f'PASS: 3 dashboards, role boundaries, existing booking flow, {len(files)} XAML files well-formed.')
print('NOTE: compile and runtime UI must be tested on Windows with .NET 8.')
