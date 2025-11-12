public interface IUpgradable
{
    UpgradesData UpgradesData { get; }

    bool CanUpgrade { get; }

    void Unlock();
}
