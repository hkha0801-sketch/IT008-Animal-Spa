using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Animal_Spa.Models;

namespace Animal_Spa.Components.Receptionist
{
    public partial class CustomerDetailView : UserControl
    {
        // Phát mã khách hàng khi yêu cầu thêm thú cưng, Action<int> nhận phương thức void có tham số int.
        public event Action<int>? AddPetRequested;

        // Lưu mã và tên khách hàng đang được hiển thị.
        public int? CurrentCustomerId { get; private set; }
        public string CurrentCustomerName { get; private set; } = string.Empty;

        // Khởi tạo các thành phần giao diện chi tiết khách hàng.
        public CustomerDetailView()
        {
            InitializeComponent();
        }

        // Tải dữ liệu mẫu theo mã khách hàng và hiển thị thông tin cùng danh sách thú cưng.
        public void LoadCustomer(int customerId)
        {
            CustomerDetailItem customer = BuildMockCustomer(customerId);
            CurrentCustomerId = customer.Id;
            CurrentCustomerName = customer.Name;

            CustomerNameText.Text = customer.Name;
            CustomerPhoneText.Text = customer.Phone;
            AddressText.Text = customer.Address;
            NoteText.Text = customer.Note;

            // ItemsSource cung cấp danh sách thú cưng cho vùng biểu tượng và vùng chi tiết.
            CustomerPetIconsControl.ItemsSource = customer.Pets;
            PetItemsControl.ItemsSource = customer.Pets;
        }

        // Tạo dữ liệu khách hàng mẫu và các thú cưng tương ứng với mã được truyền vào.
        private static CustomerDetailItem BuildMockCustomer(int customerId)
        {
            if (customerId == 3)
            {
                return new CustomerDetailItem
                {
                    Id = 3,
                    Name = "KHA BÙI",
                    Phone = "+84xxxxxx647",
                    Address = "335, LONG HÀO, THỦ ĐỨC",
                    Note = "YÊU CẦU CAO VỀ VỆ SINH LÔNG",
                    Pets = new List<PetDetailItem>
                    {
                        CreatePet(301, "BICKY", "Cat", "MIU", "12/06/2021", "CAT.png"),
                        CreatePet(302, "BICKY", "Cat", "MIU", "12/06/2021", "CAT.png"),
                        CreatePet(303, "BICKY", "Dog", "GOLDEN", "12/06/2021", "DOG.png")
                    }
                };
            }

            int petCount = customerId == 5 ? 2 : 1;
            var pets = new List<PetDetailItem>
            {
                CreatePet(customerId * 100 + 1, "MILO", "Dog", "CORGI", "10/08/2022", "DOG.png")
            };

            if (petCount > 1)
            {
                pets.Add(CreatePet(customerId * 100 + 2, "MIMI", "Cat", "MIU", "18/03/2023", "CAT.png"));
            }

            return new CustomerDetailItem
            {
                Id = customerId,
                Name = "VÕ HẢI",
                Phone = "+84xxxxxx647",
                Address = "THỦ ĐỨC, TP. HỒ CHÍ MINH",
                Note = "CHƯA CÓ GHI CHÚ ĐẶC BIỆT",
                Pets = pets
            };
        }

        // Tạo đối tượng thú cưng và đường dẫn biểu tượng từ thông tin được truyền vào.
        private static PetDetailItem CreatePet(
            int id,
            string name,
            string type,
            string breed,
            string dateOfBirth,
            string iconFile)
        {
            return new PetDetailItem
            {
                Id = id,
                Name = name,
                Type = type,
                Breed = breed,
                DateOfBirth = dateOfBirth,
                AnimalIcon = $"/Animal-Spa;component/Asset/animal/{iconFile}"
            };
        }

        // Hiển thị thông báo về chức năng sửa thông tin cho khách hàng hiện tại.
        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCustomerId is int customerId)
            {
                MessageBox.Show(
                    $"Edit Customer component will be connected for customer #{customerId}.",
                    "Animal Spa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // Phát sự kiện AddPetRequested kèm mã khách hàng hiện tại khi nhấn thêm thú cưng.
        private void AddPet_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCustomerId is int customerId)
            {
                AddPetRequested?.Invoke(customerId);
            }
        }
    }
}
