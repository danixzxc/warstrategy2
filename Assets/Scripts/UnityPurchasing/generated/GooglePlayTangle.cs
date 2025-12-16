// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("DsZXPTzcuWyOdNoMLAWbofcH75h6SGU+K7RGMWZFXI6zGl0nExAFKZIUEeQLo1mn7y+0ExPnOXRXYljdvHiRI7vP5sbWE3F0pH4mmSB27574g8QdjYFGb6t+fuL6BeuyIKia568SDWltmfWTyfwVFs/i2s7wvL+rd3iW744WboCxIsex180oVL+SDDmEblmwaDJ09B1Yj0Xsu6RUUWep7JbwlG/Q+PP8zuDDtQlmCMPu95uMGKFwt0b+vYm4p4BKUk7yBm1p7LWgEpGyoJ2WmboW2BZnnZGRkZWQk6rUUrquPH3EJRJxJQqOOkUzQzm2SEF5N/KM74Du+0VBwis69dYDXkASkZ+QoBKRmpISkZGQKh0ldWVVquOX4EdG0izt4ZKTkZCR");
        private static int[] order = new int[] { 4,5,11,3,8,6,9,7,11,13,11,11,12,13,14 };
        private static int key = 144;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
