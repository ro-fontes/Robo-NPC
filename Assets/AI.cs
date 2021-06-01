using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //Criando as variaveis
    [Tooltip("Transform do Player")]
    public Transform player;
    [Tooltip("Spawn da bala")]
    public Transform bulletSpawn;
    [Tooltip("Barra de vida")]
    public Slider healthBar;
    [Tooltip("Prefab da Bala")]
    public GameObject bulletPrefab;//Prefab da bala

    NavMeshAgent agent;
    public Vector3 destination; // Vetor de destino
    public Vector3 target;      // posicao do inimigo.
    float health = 100.0f;
    float rotSpeed = 5.0f;
    float visibleRange = 80.0f;
    float shotRange = 40.0f;

    void Start()
    {
        //Pegando o NavMeshAgent do Gameobject
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        //Chamando repedidamente a funcao UpdateHealth
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //Posicao da barra de vida Em relacao a tela
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    void UpdateHealth()
    {
       if(health < 100)
        //se a vida for menor que 100 adiciona vida
        health ++;
    }

    void OnCollisionEnter(Collision col)
    {
        //ao toma tiro, reduz a vida
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }

    //Setando uma posicao aleatoria com o SetDestination
    [Task]
    public void PickRandomDestination()
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)); // definindo o destino
        agent.SetDestination(dest); //Setando o destino do inimigo 
        Task.current.Succeed();
    }

    //Movendo o pesonagem ate a posicoes
    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        }
        if(agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    //Patrol
    [Task]
    public void PickDestination(int x, int z)
    {
        Vector3 dest = new Vector3(x, 0, z);
        //setando o destino do navmesh para uma posicao definida
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position; //Pegando a posicao do jogador
        Task.current.Succeed();
    }

    [Task]
    public bool Fire()
    {
        //Instanciando a bala
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        //Adicionando forca a bala
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        return true;
    }
    [Task]
    public void LookAtTarget()
    {
        //Direcao que esta apontando
        Vector3 direction = target - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        //Verificando o angulo entre meu personagem e o Jogador
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("angle+{0}", Vector3.Angle(this.transform.forward, direction));
        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)
        { Task.current.Succeed(); }
    }

    [Task]
    bool SeePlayer()
    {
        //Distancia do player com o personagem
        Vector3 distance = player.transform.position - this.transform.position;
        bool seeWall = false;

        //Criando um rayCast
        RaycastHit hit;
        Debug.DrawRay(this.transform.position, distance, Color.red);
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            //se o raycast colidir com a parede
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        if (distance.magnitude < visibleRange && !seeWall) 
            return true;
        else
            return false;
    }

    //Funcao para rotacionar o Player
    [Task]
    bool Turn(float angle)
    {
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p;
        return true;
    }
    [Task]
    public bool IsHealthLessThan(float health)
    {
        return this.health < health;//Verificando se ele esta vivo
    }

    [Task]
    public bool Explode()
    {
        Destroy(healthBar.gameObject);//Destroi a barra de vida
        Destroy(this.gameObject);// destroi o inimigo(Este gameobject)
        return true;
    }
}

