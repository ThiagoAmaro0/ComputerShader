using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicManager : MonoBehaviour
{
    struct Ball
    {
        public Vector3 pos;
        public float velocity;
        public float mass;
        public Color color;
    }

    [SerializeField] ComputeShader computeShader;
    [SerializeField] GameObject pref;

    float minMass, maxMass, minVelocity, maxVelocity; //limites dos parâmetros aleatórios
    int count;//largura da grid de bolas
    bool startGPU, startCPU; //flag do inicio do processamento
    float lastTime;//variavel para o calculo do delta 
    float endTime; // variaveis que calculam o tempo de execução

    Ball[] data;//lista de dados
    GameObject[] objects;//lista de gameObjects

    //variaveis de input
    string _count = "10";
    string _minMass = "1";
    string _maxMass = "7";
    string _minVelocity = "1";
    string _maxVelocity = "3";

    [SerializeField] float time = 0;

    void Start()
    {

    }

    void Update()
    {
        
    }
    //desenha os botÃµes e os campos de texto
    private void OnGUI()
    {
        if (data == null)
        {
            GUI.Label(new Rect(0, 70, 200, 30), "Numero largura da Grid de bolas:");
            _count = GUI.TextField(new Rect(0, 100, 200, 30), _count);

            GUI.Label(new Rect(0, 170, 200, 30), "Massa minima/maxima:");
            _minMass = GUI.TextField(new Rect(0, 200, 100, 30), _minMass);
            _maxMass = GUI.TextField(new Rect(100, 200, 100, 30), _maxMass);

            GUI.Label(new Rect(0, 270, 200, 30), "Velocidade minima/maxima:");
            _minVelocity = GUI.TextField(new Rect(0, 300, 100, 30), _minVelocity);
            _maxVelocity = GUI.TextField(new Rect(100, 300, 100, 30), _maxVelocity);

            if (GUI.Button(new Rect(0, 0, 100, 50), "Criar Bolas"))
            {
                count = int.Parse(_count);
                minMass = int.Parse(_minMass);
                maxMass = int.Parse(_maxMass);
                minVelocity = int.Parse(_minVelocity);
                maxVelocity = int.Parse(_maxVelocity);
                CreateBalls();
            }
        }
        else if (!startGPU && !startCPU)
        {
            if (GUI.Button(new Rect(110, 0, 100, 50), "Iniciar GPU"))
            {
                PhysicsGPU();
                time = 0;
                endTime = 0;
                lastTime = 0;
            }
            if (GUI.Button(new Rect(220, 0, 100, 50), "Iniciar CPU"))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i].color = new Color(0, 1, 0, 1);
                }
                startCPU = true;
                time = 0;
                endTime = 0;
                lastTime = 0;
            }
        }
        else
        {
            if (GUI.Button(new Rect(0, 330, 100, 50), "Resetar"))
            {
                data = null;
                foreach (GameObject gameObject in objects)
                {
                    Destroy(gameObject);
                }
                objects = new GameObject[count * count];
                startGPU = false;
                startCPU = false;
            }
        }
    }

    //cria a grid de bolas conforme os dados inseridos 
    void CreateBalls()
    {
        objects = new GameObject[count * count];
        data = new Ball[count * count];
        for (int i = 0; i < count; i++)
        {
            float posX = -count / 2 + i;
            for (int j = 0; j < count; j++)
            {
                float posY = -count / 2 + j;
                GameObject go = Instantiate(pref, new Vector3(posX * 2.5f, 100, posY * 2.5f), Quaternion.identity);
                objects[j + i * count] = go;
                data[j + i * count] = new Ball();
                data[j + i * count].pos = go.transform.position;
                data[j + i * count].velocity = Random.Range(minVelocity, maxVelocity);
                data[j + i * count].mass = Random.Range(minMass, maxMass);
            }
        }
    }

    //executa o kernel Start
    void PhysicsGPU()
    {
        int count = 0;
        foreach (Ball b in data)
        {
            if (b.color == Color.red)
                count++;
        }
        if (count == data.Length)
        {
            CreateBalls();
        }
        else
        {
            int totalSize = 3 * sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float) * 4;

            ComputeBuffer computeBuffer = new ComputeBuffer(data.Length, totalSize);
            computeBuffer.SetData(data);


            computeShader.SetBuffer(0, "balls", computeBuffer);
            computeShader.SetFloat("time", Time.fixedTime);
            computeShader.Dispatch(0, data.Length / 10, 1, 1);

            computeBuffer.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                objects[i].GetComponent<MeshRenderer>().material.SetColor("_Color", data[i].color);
            }
            computeBuffer.Dispose();

            startGPU = true;
        }
    }
}
