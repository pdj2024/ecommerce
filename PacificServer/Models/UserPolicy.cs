namespace Models
{
    public class UserPolicy
    {
        public const string VIEW_TAGIHAN = "VIEW_TAGIHAN";
        public const string ADD_TAGIHAN = "ADD_TAGIHAN";
        public const string EDIT_TAGIHAN = "EDIT_TAGIHAN";
        public const string DELETE_TAGIHAN = "DELETE_TAGIHAN";

        public static List<string> GetPolicies()
        {
            return new List<string>
            {
                VIEW_TAGIHAN,
                ADD_TAGIHAN,
                EDIT_TAGIHAN,
                DELETE_TAGIHAN

            };
        }
    }
}
