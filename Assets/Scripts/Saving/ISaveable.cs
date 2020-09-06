
public interface ISaveable
{
    IComponentSaveData Save();
    void Load(IComponentSaveData data);    
}