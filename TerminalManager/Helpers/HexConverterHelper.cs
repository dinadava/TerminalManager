namespace TerminalManager.Helpers
{
    public class HexConverterHelper
    {
        public static string HexToDecimal(string text)
        {
            string returnValue = "";
            foreach (string hex in text.Split())
            {
                returnValue += int.Parse(hex, System.Globalization.NumberStyles.HexNumber).ToString();
            }
            return returnValue;
        }
    }
}
