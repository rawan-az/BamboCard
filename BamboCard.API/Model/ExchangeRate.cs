namespace BamboCard.API.Model
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}
