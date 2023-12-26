namespace BOL
{
    public class ALCSFiles
    {
        public int ALCSId { get; set; }
        public DateOnly Date { get; set; }

        public string FileName { get { return "ALCS_" + this.ALCSId + "_" + this.Date.ToString("yyyyddmm"); } }
    }
}
