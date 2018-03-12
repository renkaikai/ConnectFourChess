using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public Image maskImage;
	public GameObject StartBtn;
	public GameObject victImage;
	public GameObject defeatImage;
	[SerializeField]
	private GameObject gameController;
	private float timer=0;
	private float totalTime=2;
	private bool startFade=false;

	private GameController gc;
	// Use this for initialization
	void Start () 
	{
		maskImage.fillAmount=1;
		
		gc=gameController.GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () 
	{	
		//点完开始游戏, 黑色遮罩消失，并将游戏逻辑脚本激活
		if(startFade)
		{
			timer+=Time.deltaTime;
			maskImage.fillAmount=(totalTime-timer)/totalTime;
			if(maskImage.fillAmount==0)
			{
				gameController.SetActive(true);
				startFade=false;
			}
		}
		//如果玩家胜利, 黑色遮罩出现,胜利ui出现
		if(gc.result=="你赢了")
		{			
			timer-=Time.deltaTime;
			maskImage.fillAmount=(totalTime-timer)/totalTime;
			victImage.SetActive(true);
		}
		//如果玩家失败，黑色遮罩出现，失败ui出现
		if(gc.result=="你输了")
		{
			timer-=Time.deltaTime;
			maskImage.fillAmount=(totalTime-timer)/totalTime;
			defeatImage.SetActive(true);
		}
		
	}

	//用来管理黑色遮罩图片的出现和消失
	public void FadeMask()
	{	
		//点击使开始游戏按钮消失		
		StartBtn.gameObject.GetComponent<Image>().enabled=false;		
		if(maskImage.fillAmount==1)
		{
			startFade=true;
		}
	}

}
