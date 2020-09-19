
public interface ISaveableComponent
{
    IComponentSaveData Save();
    void Load(IComponentSaveData data);    
}