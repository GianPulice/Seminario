using System.Collections;
using UnityEngine;

public class ClientStateEating<T> : State<T>
{
    private ClientModel clientModel;
    private ClientView clientView;

    private ClientStateLeave<T> clientStateLeave;

    private float eatingTime = 0f;

    private bool isEating = false;

    public bool IsEating { get => isEating; set => isEating = value; }


    public ClientStateEating(ClientModel clientModel, ClientView clientView, ClientStateLeave<T> clientStateLeave)
    {
        this.clientModel = clientModel;
        this.clientView = clientView;
        this.clientStateLeave = clientStateLeave;
    }


    public override void Enter()
    {
        base.Enter();
        Debug.Log("Eating");

        clientModel.StartCoroutine(WaitFrameToPlaySound());

        clientView.SetSpriteTypeName("SpriteEating");
    }

    public override void Execute()
    {
        base.Execute();

        eatingTime += Time.deltaTime;

        if (eatingTime >= clientModel.ClientData.MaxTimeEating)
        {
            isEating = false;
            clientStateLeave.CanLeave = true;
            eatingTime = 0f;
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();

        clientModel.AudioSource3D.Stop();

        isEating = false;
        clientView.Anim.transform.position += Vector3.down * 0.38f;
        eatingTime = 0f;
    }


    private IEnumerator WaitFrameToPlaySound()
    {
        yield return new WaitForSecondsRealtime(0.35f);

        AudioClip clipEating;
        int randomSound = Random.Range(0, 2);

        if (randomSound == 0)
        {
            clipEating = AudioManager.Instance.GetSFX("Eating1");
        }

        else
        {
            clipEating = AudioManager.Instance.GetSFX("Eating2");
        }

        clientModel.AudioSource3D.clip = clipEating;
        clientModel.AudioSource3D.Play();
    }
}
