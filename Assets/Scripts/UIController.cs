using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public Image m_maskImage;
	public GameObject m_StartBtn;
	public GameObject m_victImage;
	public GameObject m_defeatImage;
	public GameObject m_drawImage;
	[SerializeField]
	private GameObject m_gameController;
	private float m_timer=0;
	private float m_totalTime=2;
	private bool m_startFade=false;
	private GameController m_gc;
	//title动画
	private Animator m_titleAnim;
	// Use this for initialization
	void Start () 
	{
		m_maskImage.fillAmount=1;
		
		m_gc=m_gameController.GetComponent<GameController>();

		m_titleAnim=GameObject.Find("TitleImage").GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
	{	
		//点完开始游戏, 黑色遮罩消失，并将游戏逻辑脚本激活
		if(m_startFade)
		{
			m_timer+=Time.deltaTime;
			m_maskImage.fillAmount=(m_totalTime-m_timer)/m_totalTime;
			if(m_maskImage.fillAmount==0)
			{
				m_gameController.SetActive(true);
				m_startFade=false;
			}
		}
		//如果玩家胜利, 黑色遮罩出现,胜利ui出现
		if(m_gc.result=="你赢了")
		{			
			ShowMask();
			m_victImage.SetActive(true);
			m_titleAnim.SetBool("down",true);
		}
		//如果玩家失败，黑色遮罩出现，失败ui出现
		if(m_gc.result=="你输了")
		{	
			ShowMask();			
			m_defeatImage.SetActive(true);
			m_titleAnim.SetBool("down",true);			
		}
		if (m_gc.result=="平局")
		{			
			ShowMask();			
			m_drawImage.SetActive(true);
			m_titleAnim.SetBool("down",true);			
		}
		
	}

	//用来管理黑色遮罩图片的出现和消失
	public void FadeMask()
	{	
		//点击使开始游戏按钮消失		
		m_StartBtn.gameObject.GetComponent<Image>().enabled=false;		
		if(m_maskImage.fillAmount==1)
		{
			m_startFade=true;
		}
		//title上移
		m_titleAnim.SetBool("up",true);
	}

	//显示黑色遮罩
	private void ShowMask()
	{
		m_timer-=Time.deltaTime;
		m_maskImage.fillAmount=(m_totalTime-m_timer)/m_totalTime;
	}
}
