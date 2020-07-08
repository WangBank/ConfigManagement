namespace ConfigManagement.Models
{
    public class ResultData
    {
        public string Code { get; set; }
        public string Msg { get; set; }

        public object Data { get; set; }

        public static ResultData CreateSuccessResult(string msg = null,object data = null)
        {

            return new ResultData
            {
                Code = "0",
                Msg = msg,
                Data = data
            };
        }

        public static ResultData CreateResult(string code, string msg, object data)
        {
            return new ResultData
            {
                Code = code,
                Msg = msg,
                Data = data
            };
        }
    }
}
