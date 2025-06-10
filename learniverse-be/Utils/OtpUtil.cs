namespace learniverse_be.Utils
{
  public static class OtpUtil
  {
    public static string GenerateOtp(int length = 6)
    {
      var rng = new Random();
      return string.Concat(Enumerable.Range(0, length).Select(_ => rng.Next(10)));
    }
  }
}
