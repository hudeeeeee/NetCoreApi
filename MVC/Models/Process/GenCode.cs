namespace MVC.Models.Process
{
    public class GenCode
    {
        // Sinh chuỗi mã có prefix và số thứ tự
        public string GenerateCode(string prefix, int number)
        {
            // VD: prefix = "KH", number = 1 => KH001
            return prefix + number.ToString("D3"); // D3 = padding 3 số
        }

        // Sinh mã random (chữ và số)
        public string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
