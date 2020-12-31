using static Bomber.Items.Pickup;

namespace Bomber.Items
{
    public interface IPowerUp
    {
        void ApplyPowerUp(PowerUp details);
    }
}