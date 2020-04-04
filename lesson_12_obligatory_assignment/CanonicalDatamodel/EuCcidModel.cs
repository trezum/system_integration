namespace CanonicalDataModel
{
    public class EuCcidModel
    {
        public string ChristianName { get; set; }
        public string FamilyName { get; set; }
        public string EuCcid { get; set; }
        public Gender Gender { get; set; }
        public string StreetPlusNumberOfHouse { get; set; }
        public string ApartmentNumber { get; set; }
        public string County { get; set; }
        public string City { get; set; }
        public string BirthCountry { get; set; }
        public string CurrentLivingInCountry { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }
}
