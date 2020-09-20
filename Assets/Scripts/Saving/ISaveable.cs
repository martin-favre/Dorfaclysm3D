
public interface ISaveableComponent
{
    IGenericSaveData Save();
    void Load(IGenericSaveData data);    
}