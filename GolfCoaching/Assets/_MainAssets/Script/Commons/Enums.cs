namespace Enums
{
    public enum EDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum ESceneType
    {
        Loading = -1,
        Boot,
        Login,
        ProSelect,
        ModeSelect,
        Setup,
        LessonMode,
        PracticeMode,
        AICoaching,
        FocusCoaching,
        Mirror,
        Range,
        SetupMode,
        Studio,
        Recording
    }

    public enum ESetupMode
    {
        None = -1,
        Pose,
        Club,
        Confirm
    }

    public enum EStep
    {
        None = -1,
        Realtime,
        Preview
    }

    public enum EPoseDirection
    {
        Front,
        Side,
        All
    }

    public enum SWINGSTEP
    {
        CHECK = -2,
        READY = -1,
        ADDRESS = 0,
        TAKEBACK = 1,
        BACKSWING = 2,
        TOP = 3,
        DOWNSWING = 4,
        IMPACT = 5,
        FOLLOW = 6,
        FINISH = 7
    }

    public enum EClub
    {
        None = -1,
        Driver,
        Wood,
        ShortIron,
        MiddleIron,
        LongIron,
        Approach,
        Putter,
    }

    public enum EStance
    {
        None = -1,
        Grib,
        Address,
        Half,
        ThreeQuarter,
        Full,
        Takeback,
        Backswing,
        Top,
        Downswing,
        Impact,
        Follow,
        Finish,
    }

    public enum EImageType
    { 
        None = -1,
        Profile,
        Thumbnail,
    }

    public enum EFilter
    {
        None = -1,
        Basic,
        Swing,
        Range,
        Short,
        Female,
        Body,
        Real,
        Front,
        Repeat,
        Club
    }

    public enum EMODE
    {
        Lesson,
        Practice,
        AITest,
        personalized,
        Mirror,
        LaunchMonitor
    }

    public enum EArraySortMode
    {
        View,
        Recently,
        Favorite,
        ManyVideo,
    }

    public enum EVideoSourceType
    {
        None = -1,
        Recommend,
        Recently,
        Best
    }

    public enum ELessonState
    {
        None = -1,
        List,
        Play,
        End,
        Recently,
        Best
    }

    public enum ESwingType
    {
        None = -1,
        Full = 0,
        ThreeQuarter = 1,
        Half = 2,
    }

    public enum ERecordingType
    {
        None = -1,
        Profile,
        Lesson,
        Practice,
    }
}