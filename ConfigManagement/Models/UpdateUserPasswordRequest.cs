namespace ConfigManagement.Models
{
    public class UpdateUserPasswordRequest
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
