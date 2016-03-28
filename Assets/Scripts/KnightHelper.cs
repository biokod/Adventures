using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KnightHelper : MonoBehaviour
{

    public float Speed { get; set; }        // Скорость передвижения.
    public sbyte Direction { get; set; }     // Направление.
    public int HealthPoint { get; set; }    // Уровень здоровья.
    public float RotationSpeed { get; set; }    // Скорость вращения.
    public float ReloadTime { get; set; }   // Время перезарядки (для повторной атаки)
    public float RelaxTime { get; set; }    // Время возобновления.
    public int Damage { get; set; }     // Сила атаки.
    public EState Current { get; set; }     // Текущее состояние.

    Animator anim;
    Slider healthBar;


    void Start()
    {
        Current = EState.Idle;  // При старте игры, текущее состояние: бездействие.

        HealthPoint = 100;
        Speed = 2.6F;
        RotationSpeed = 118.0F;
        Direction = 1;
        Damage = 25;

        ReloadTime = 0.65F;
        RelaxTime = 0.45F;

        anim = GetComponent<Animator>();
        healthBar = FindObjectOfType<Slider>();

        Invoke("HealthRegenerate", 1.0F);
    }

    void Update()
    {
        ChangeState();
        Move();
        Animate();

        //Alive();
        ShowHealthBar();
    }

    void ChangeState()
    {
        if (HealthPoint <= 0) Current = EState.Dead;    // Если закончилось здоровье, значит игрок мертв.
        if (Current == EState.Dead) return;             // Если игрок мертв, он не может сам изменить свое состояние.
        if (Current == EState.Damage) return;           // Если игрок оглушен, он тоже не может выполнять никаких деййствий.

        if (Current != EState.Attack)       // Если игрок не бьет, значит он может передвигаться.
        {
            if (Input.GetKey(KeyCode.W))
            {
                Direction = 1;          // Направление "Вперед"
                Current = EState.Walk;
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                Current = EState.Idle;
            }

            if (Input.GetKey(KeyCode.S))
            {
                Direction = -1;         // Направление "Назад"
                Current = EState.Walk;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                Current = EState.Idle;
                Direction = 1;          // Если игрок не идет назад, значит его направление -  "Вперед".
            }
        }

        // Если игрок еще никого не бьет, значит можно бить.
        if (Input.GetMouseButtonDown(0) && Current != EState.Attack)
        {

            Current = EState.Attack;                // Переводим игрок в состояние атаки.
            Invoke("Attack", ReloadTime * 0.75F);   // Запускаем таймер атаки.
            Invoke("ToIdle", ReloadTime);           // Запускаем таймер возобновления состояния.
        }
    }


    void Move()
    {
        if (Current == EState.Dead) return;     // Если игрок мертв, ходить нельзя.
        if (Current == EState.Damage) return;   // Если игрок оглушен, ходить тоже нельзя.

        if (Input.GetButton("Horizontal"))  // Поворот игрока.
            transform.Rotate(transform.up, Time.smoothDeltaTime * RotationSpeed * Input.GetAxis("Horizontal") * Direction);

        if (Current != EState.Walk) return; // Если наше состояние не Ходьба, ходить не будем.

        // Передвижение.
        Vector3 target = transform.position + (transform.forward * Direction);
        transform.position = Vector3.MoveTowards(transform.position, target, Time.smoothDeltaTime * Speed);
    }

    void Attack()
    {
        if (Current != EState.Attack) return;   // Если наше состояние не Атака, значит не бьем.

        // Получаем все коллыйдеры в зоне поражения игрока.
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward, 0.6F);

        foreach (var unit in colliders)
        {
            if (unit.gameObject.GetComponent<SlimeHelper>())    // Если это слизень, то будем его бить
            {
                if (unit.isTrigger) return; // Если это триггер слизня, значит еще не подошли. Бить рано.

                SlimeHelper slime = unit.gameObject.GetComponent<SlimeHelper>();

                if (slime.Current == EState.Dead) return;   // Если слизень мертв, нету смысла бить.

                slime.HealthPoint -= Damage;            // Бьем.

                if (slime.Current != EState.Attack)     // Если слизень не в состоянии Атака.
                {
                    slime.Current = EState.Damage;          // Слизень оглушен.
                    slime.Invoke("ToIdle", slime.RelaxTime);// Запускаем тамер возобновления состояния слизня.
                }

                unit.gameObject.GetComponent<Rigidbody>().AddForce((unit.transform.forward * (-2.7F)), ForceMode.Impulse);
                return;     // Завершаем метод, чтобы случайно не нанести двойной урон.
            }
        }
    }

    void ToIdle()
    {
        Current = EState.Idle;
    }

    void Animate()
    {
        switch (Current)    // Если текущее состояние:
        {
            case EState.Idle:   // Бездествие
                {
                    anim.SetInteger("State", (int)EState.Idle); // Воспроизводим анимацию бездествия.
                    break;
                }
            case EState.Walk:   // Ходьба
                {
                    anim.SetInteger("State", (int)EState.Walk); // Воспроизводим анимацию ходьбы.
                    break;
                }
            case EState.Attack: // Атака
                {
                    anim.SetInteger("State", (int)EState.Attack); // Воспроизводим анимацию атаки.
                    break;
                }
            case EState.Damage: // Урон
                {
                    anim.SetInteger("State", (int)EState.Damage); // Воспроизводим анимацию урона.
                    break;
                }
            case EState.Dead:   // Смерть
                {
                    anim.SetInteger("State", (int)EState.Dead); // Воспроизводим анимацию смерти.
                    break;
                }
        }
    }

    void Alive()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HealthPoint = 100;
            Current = EState.Idle;
        }
    }

    void HealthRegenerate()
    {
        if (HealthPoint < 100) HealthPoint++;
        Invoke("HealthRegenerate",1.0F);
    }

    void ShowHealthBar()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        pos.y += 160;
        healthBar.transform.position = pos;
        healthBar.value = HealthPoint;
    }
}
