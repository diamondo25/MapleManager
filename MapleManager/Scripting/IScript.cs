namespace MapleManager
{
    public interface IScript
    {
        string GetScriptName();
        void Start(ScriptNode mainScriptNode);
        void Stop();
    }
}
