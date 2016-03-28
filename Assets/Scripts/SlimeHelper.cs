using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlimeHelper : MonoBehaviour
{

    public float Speed { get; set; }        // Скорость передвижения.
    public float RotationSpeed { get; set; }    // Скорость поворота.
    public float ReloadTime { get; set; }       //  Време перезарядки (для повторной атаки)
    public float RelaxTime { get; set; }      // Время возобновления.
    public float DamageRadius { get; set; }     // Радиус поражения.
    public int HealthPoint { get; set; }    // Уровень здоровья
    public int Damage { get; set; }     // Сила атаки.

    public bool ReadyToAttack { get; set; } // Готовность к атаке.

    public GameObject Target { get; set; }  // Цель.
    public EState Current { get; set; }     // Текущее состояние.

    public Collider SelfCollider { get; set; }

    Animator anim;
    GameHelper gameHelper;

    void Start()
    {
        Current = EState.Idle;  // При старте игры, текущее состояние: бездействие.

        HealthPoint = 100;
        Speed = 1.5F;
        RotationSpeed = 2.2F;
        DamageRadius = 1.5F;
        Damage = 10;
        Target = null;

        ReadyToAttack = true;

        ReloadTime = 0.5F;
        RelaxTime = 0.3F;

        anim = GetComponent<Animator>();
        gameHelper = FindObjectOfType<GameHelper>();

        Collider[] Colliders = GetComponents<SphereCollider>();

        foreach (Collider collider in Colliders)
        {
            if (!collider.isTrigger) SelfCollider = collider;
        }
    }

    void Update()
    {
       //Debug.Log(Current + " " + HealthPoint);

        ChangeState();
        Move();
        Animate();

        Death();
    }

    void ChangeState()
    {
        if (HealthPoint <= 0) Current = EState.Dead;    // Если больше нет здоровья, значит слизень мертв.

        if (Current == EState.Dead) return;             // Если слизень мертв, он не может сам изменить свое состояние.

        if (Target == null) Current = EState.Idle;      // Если нету цели, бездествуем.

        if (Target != null && Current != EState.Damage) // Если есть цель и слизень не оглушен, можно действовать.
        {
            if (Current == EState.Attack) return; // Если слизень атакует, то, в данный момент, больше ничего не может делать.

                                // Если цель в зоне поражения, можно попробовать атаковать.
            if (Vector3.Distance(transform.position, Target.transform.position) <= DamageRadius)
            {
                if (ReadyToAttack)  // Если слизень готов к атаке, можно атаковать.
                {
                    Current = EState.Attack;    // Переходим в состояние атаки.
                    ReadyToAttack = false;      // Больше не готовы к атаке, чтобы не ударить без перезарядки.
                    Invoke("Attack", ReloadTime * 0.75F);   // Запускаем таймер атаки.
                    Invoke("ToIdle", ReloadTime);           // Запускаем таймер возобновления состояния.
                    Invoke("SetReadyToAttack", ReloadTime * 4.0F);  // Запускаем тамер возобновления готовности к атаке.
                }

                return; // Завершаем работу метода, чтобы никуда не идти, если бьем.
            }

            Current = EState.Walk;  // Если не атакуем цель, значит идем за ней.
        }
    }

    void Move()
    {
            // Если слизень мертв или оглушен, он не может передвигаться.
        if (Current == EState.Dead || Current == EState.Damage) return;

        if (Target != null) // Если цель не отсутствует.
        {
            SmoothLookAt(Target.transform.position, RotationSpeed); // Поворачиваемся к цели.

            if (Current == EState.Attack) return;   // Если атакуем, то не можем подойти.
                                                    // Если достаем чтобы ударить, то нету смысла подходить ближе.
            if (Vector3.Distance(transform.position, Target.transform.position) <= DamageRadius) return;

                                                    // Если же цель вне радиуса поражения, следуем за ней.
            transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Time.smoothDeltaTime * Speed);
        }
    }

    void Attack()
    {
        if (Current != EState.Attack) return;   // Если состояние не Атака, то не будем бить.

                                                // Получаем все колладеры в области поражения.
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward, 0.7F);

        foreach (var unit in colliders)
        {
            if (unit.gameObject.GetComponent<KnightHelper>()) // Если это игрок.
            {
                KnightHelper knight = unit.gameObject.GetComponent<KnightHelper>();

                knight.HealthPoint -= Damage;       // Бьем его.

                if (knight.Current != EState.Attack) // Если цель не в состоянии Атака.
                {
                    knight.Current = EState.Damage;     // Изменяем его состояние на Оглушен.
                    knight.Invoke("ToIdle", knight.RelaxTime);  // Запускаем таймер возобновления состояния.
                }

                unit.gameObject.GetComponent<Rigidbody>().AddForce(((unit.transform.position - transform.position)* 5.5f).normalized , ForceMode.Impulse);
                return;                             // Завершаем работу метода, чтобы случайно не нанести двоной урон.
            }
        }
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

    void ToIdle()
    {
        Current = EState.Idle;
    }

    void SetReadyToAttack()
    {
        ReadyToAttack = true;
    }

    void Death()
    {
        if (Current != EState.Dead) return; // Если слизень не мертв, завершаем работу метода.

        GetComponent<Rigidbody>().isKinematic = true;
        SelfCollider.isTrigger = true;

        Destroy(gameObject, 2.5F);          // Если же слизень мертв, уничтожаем его через 4 сек.
    }

    void SmoothLookAt(Vector3 target, float smooth)
    {
        Vector3 dir = target - transform.position;
        dir = new Vector3(dir.x, 0, dir.z);
        transform.forward = Vector3.Slerp(transform.forward, dir, Time.deltaTime * smooth);
    }


    void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.GetComponent<KnightHelper>()) // Если в область видимости попал именно игрок.
        {
            Target = target.gameObject; // Значит он наша цель.
        }
    }

    void OnTriggerExit(Collider target)
    {
        if (target.gameObject.GetComponent<KnightHelper>()) // Если игрок вышел из области видимости.
        {
            Target = null; // Значит цели нет.
        }
    }

    void OnTriggerStay(Collider target)
    {
        if (target.gameObject.GetComponent<KnightHelper>())
        {
            // Если игрок в поле зрения, но он мертв.
            if (target.gameObject.GetComponent<KnightHelper>().Current == EState.Dead)

                Target = null;  // То цель отсутствует.

            else    // Если же игрок жив.

                Target = target.gameObject; // Значит он - наша цель.
        }
    }

    void OnDestroy()
    {
        gameHelper.KillCounter++;
    }
}
