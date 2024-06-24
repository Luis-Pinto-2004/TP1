using System.Collections;
using System.Diagnostics;
using System.Security.Principal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed = 10f;
    public float maxForwardSpeed = 35f; // Velocidade máxima que o jogador pode alcançar
    public float speedIncreaseDuration = 100f;
    private float elapsedTime = 0f;
    private int desiredLane = 1;
    public float laneDistance = 4f; 

    public float jumpForce = 8f;
    public float gravity = -24f;

    public Animator animator;
    
    private bool isSliding = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
{
    if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
        return;

    animator.SetBool("isGameStarted", true);
    animator.SetBool("isGrounded", controller.isGrounded);

    // Movimento para frente constante
    direction.z = forwardSpeed;

    // Verificar se o jogador está no chão
    if (controller.isGrounded)
    {
        // Verificar se a tecla de salto foi pressionada
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Iniciar o salto apenas se estiver no chão
            direction.y = jumpForce;
        }
    }
    else
    {
        // Aplicar gravidade quando o jogador não estiver no chão
        direction.y += gravity * Time.deltaTime;
    }

    if (Input.GetKeyDown(KeyCode.DownArrow) && !isSliding)
    {
        StartCoroutine(Slide());
    }

    // Movimento lateral (troca de pistas)
    if (Input.GetKeyDown(KeyCode.RightArrow))
    {
        desiredLane++;
        if (desiredLane > 2)
            desiredLane = 2;
    }
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
        desiredLane--;
        if (desiredLane < 0)
            desiredLane = 0;
    }

    // Calcular o vetor de movimento com base na pista desejada
    Vector3 moveVector = Vector3.zero;
    moveVector.z = direction.z * Time.deltaTime; // Movimento para frente
    moveVector.y = direction.y * Time.deltaTime; // Movimento vertical (gravidade ou salto)

    // Calcular a posição de destino com base na pista desejada
    Vector3 targetPosition = transform.position;
    targetPosition.x = Mathf.Lerp(targetPosition.x, (desiredLane - 1) * laneDistance, 10f * Time.deltaTime); // Ajustar para valores de pista (-1, 0, 1)

    // Aplicar movimento para a posição de destino
    controller.Move(moveVector + (targetPosition - transform.position));

     // Atualizar o tempo decorrido
    elapsedTime += Time.deltaTime;

    // Aumentar a velocidade gradualmente ao longo do tempo até alcançar maxForwardSpeed
    forwardSpeed = Mathf.Lerp(10f, maxForwardSpeed, elapsedTime / speedIncreaseDuration); // Ajuste o divisor (60f) conforme necessário para controlar a taxa de aumento da velocidade
}


    private void FixedUpdate()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;

        // Aplicar o movimento para frente
        controller.Move(direction * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Verificar colisão com obstáculos
        if (hit.transform.CompareTag("Obstacle"))
        {
            PlayerManager.gameOver = true;
            FindObjectOfType<AudioManager>().PlaySound("GameOver");
        }
    }

    private IEnumerator Slide()
{
    isSliding = true;
    animator.SetBool("isSliding", true);

    // Reduzir a altura do personagem e deslocar o centro para simular o deslizamento
    controller.center = new Vector3(0, -0.5f, 0);
    controller.height = 1;

    float slideDuration = 0.7f; // Duração máxima do deslizamento em segundos
    float timer = 0f;

    while (timer < slideDuration)
    {
        timer += Time.deltaTime;
        yield return null; // Esperar pelo próximo quadro
    }

    // Restaurar a altura e o centro do personagem ao estado normal
    controller.center = new Vector3(0, 0, 0);
    controller.height = 2;

    // Atualizar o estado da animação de deslizamento para false
    animator.SetBool("isSliding", false);

    // Sinalizar que o deslizamento terminou
    isSliding = false;
}


}