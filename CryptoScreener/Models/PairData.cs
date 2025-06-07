namespace CryptoScreener.Models
{
    public class PairData
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal PriceChange { get; set; }
        public decimal RSI { get; set; }
        public string RSIStatus { get; set; }
    }
}
