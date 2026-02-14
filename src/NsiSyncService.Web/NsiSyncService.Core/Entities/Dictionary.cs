namespace NsiSyncService.Core.Entities;

public class Dictionary
{
    public string Code { get; }
    public string Name { get; }
    public string CurrentVersion { get; private set; }
    public DateTime LastUpdate { get;  private set; }
    public string Law { get;  }
    
    public Dictionary(string code, string name, string currentVersion, DateTime lastUpdate, string law)
    {
        Code = code;
        Name = name;
        CurrentVersion = currentVersion;
        LastUpdate = lastUpdate;
        Law = law;
    }

    public bool NeedToUpdateVersion(string newVersion)
    {
        if (CurrentVersion != newVersion)
            return true;

        return false;
    }
    
    public void UpdateVersion(string newVersion, DateTime newLastUpdate)
    {
        CurrentVersion = newVersion;
        LastUpdate = newLastUpdate;
    }
}