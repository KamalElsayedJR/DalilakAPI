namespace API.Responses.UsedCar
{
    public class UsedCarResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string BuyerPhoneNumber { get; set; }
        public int CreatedAtYear { get; set; }
        public List<string> Images { get; set; }
    }
}
