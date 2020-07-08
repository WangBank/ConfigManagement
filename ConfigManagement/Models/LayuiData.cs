namespace ConfigManagement.Models
{
    public class LayuiData
    {
        public string Code { get; set; }

        public string Msg { get; set; }

        public int Count { get; set; }

        public object Data { get; set; }

        public static LayuiData CreateResult(int count,object data)
        {
            LayuiData layuiData = new LayuiData
            {
                Code = "0",
                Msg = "",
                Count = count,
                Data = data
            };
            return layuiData;
        }

        public static LayuiData CreateErrorResult(string msg)
        {
            LayuiData layuiData = new LayuiData
            {
                Code = "-1",
                Msg = msg,
                Count = 0,
                Data = null
            };
            return layuiData;
        }
    }
}
