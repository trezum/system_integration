namespace CprDataModel
{
    public class CprModel
    {
        public string Fornavn { get; set; }
        public string Efternavn { get; set; }
        public string Cprnr { get; set; }
        public string Adresse1 { get; set; }
        public string Adresse2 { get; set; }
        public string PostNummer { get; set; }
        public string By { get; set; }
        public ÆgteskabeligStatus ÆgteskabeligStatus { get; set; }
        public string ÆgtefælleCprNr { get; set; }
        public string[] BørnsCprNr { get; set; }
        public string[] ForældresCprNr { get; set; }
        public string LægeCvrNr { get; set; }
    }

    public enum ÆgteskabeligStatus
    {
        Unmarried,
        Married
    }
}
