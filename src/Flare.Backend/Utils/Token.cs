
namespace Flare.Backend.Utils {
    public interface Token {
        int user_id { get; }

        int type { get; }

        string name { get; }
    }

    public class UserToken : Token {
        public int type { get; }

        public int user_id { get; }

        public string name { get; }

        public UserToken(int id, string name, int rol) {
            user_id = id;
            this.name = name;
            type = rol;
        }
    }
}
