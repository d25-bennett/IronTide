using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    public Image healthbar;

    public void UpdateHealthbar(float health)
    {
        healthbar.fillAmount = health;
    }
}
// Video for code https://www.youtube.com/watch?v=FQNZwcd6FaY
