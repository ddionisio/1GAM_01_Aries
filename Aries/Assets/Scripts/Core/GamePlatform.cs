public struct GamePlatform {
    public enum Type {
        Default,
        Ouya,

        NumTypes
    }

    public static Type current = Type.Default;
}