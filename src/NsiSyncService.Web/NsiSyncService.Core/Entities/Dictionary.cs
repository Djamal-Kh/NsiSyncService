namespace NsiSyncService.Core.Entities;

public class Dictionary
{
    public string Code { get; }
    public string Name { get; }
    public string Version { get; private set; }
    public DateTime LastUpdate { get;  private set; }
    public string Law { get;  }
    
    public Dictionary(string code, string name, string version, DateTime lastUpdate, string law)
    {
        Code = code;
        Name = name;
        Version = version;
        LastUpdate = lastUpdate;
        Law = law;
    }

    public bool NeedToUpdateVersion(string newVersion)
    {
        if (Version != newVersion)
            return true;

        return false;
    }
    
    public void UpdateVersion(string newVersion, DateTime newLastUpdate)
    {
        Version = newVersion;
        LastUpdate = newLastUpdate;
    }
}