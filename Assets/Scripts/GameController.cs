using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class GameController : MonoBehaviour {
	private int m_BoardRows=6;
	private int m_BoardCols=7;
	//声明一个二维数组,用来存储棋盘数据
	private int[,] m_BoardField;
	[SerializeField]
	private GameObject m_DiscBlue;
	[SerializeField]	
	private GameObject m_DiscRed;
	//x轴上两棋子的偏移量:0.82f;y轴上两棋子偏移量:0.88f
	private float m_OffsetX=0.82f;
	private float m_OffsetY=0.88f;
	//玩家棋子跟随鼠标水平移动,需要判断是否是当前自己的棋子,AI棋子不需要跟随鼠标移动
	private GameObject m_CurrentDisc;
	//记录当前棋手的编号,用于棋手的轮换
	private int m_CurrentPlayerId=1;
	//声明一个布尔值,用来判断棋子是否下落完成
	private bool isFalling;
	//声明一个布尔值,用来判断我方是否胜利
	private bool isWin;
	//人机对战游戏结果
	public  string result="游戏还未结束";	

	// Use this for initialization
	void Start () {
		PrepareField();
		//m_CurrentDisc=AddDisc(Random.Range(1,3));
		m_CurrentDisc=AddDisc(1);
	}
	
	// Update is called once per frame
	void Update () {
		//按下p键打印当前棋盘棋子内容
		if(Input.GetKeyUp(KeyCode.P))
		{
			PrintArray();
		}

		//玩家和电脑的棋子下落
		if(m_CurrentPlayerId==1)
		{
			//棋子随鼠标水平移动
			MoveHorizontal();
			//按下鼠标左键投掷棋子
			if(Input.GetMouseButtonDown(0)&&!isFalling)
			{
				if (m_CurrentDisc)
				{
					StartCoroutine(DropDisc(m_CurrentDisc));			
				}
			}
		}
		else
		{
			if (!isFalling)
			{
				StartCoroutine(DropDisc(m_CurrentDisc));
			}
		}

		//游戏是否胜利的判断
		if (isWin)
		{
			result=(m_CurrentPlayerId==1)?"你赢了":"你输了";
			Debug.Log(result);
			return;
		}
		if(GetPossibleCol().Count==0&&!isWin)
		{
			result="平局";
			return;
		}
	}

	//棋盘数组的生成和初始化
	private void PrepareField()
	{
		m_BoardField=new int[m_BoardRows,m_BoardCols];
		for(int i=0;i<m_BoardRows;i++)
		{
			for(int j=0;j<m_BoardCols;j++)
			{
				m_BoardField[i,j]=0;
			}
		}		
	}

	//打印棋盘数组内容
	private void PrintArray()
	{
		StringBuilder m_PrintArray=new StringBuilder();
		//从上至下,从左至右遍历棋盘并打印
		for(int i=m_BoardRows-1;i>=0;i--)
		{
			for(int j=0;j<=m_BoardCols-1;j++)
			{
				m_PrintArray=m_PrintArray.Append(m_BoardField[i,j].ToString()+"  ");				
			}
			m_PrintArray.Append("\n");
		}
		Debug.Log(m_PrintArray.ToString());
	}

	//向棋盘中添加棋子
	private GameObject AddDisc(int PlayerId)
	{
		m_CurrentPlayerId=PlayerId;
		//我方棋子产生位置
		Vector3 spawnPos=Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//电脑棋子的产生位置
		if(m_CurrentPlayerId==2)
		{
			List<int> move=AI();
			if (move.Count>0)
			{
				int column=move[Random.Range(0,move.Count)];
				spawnPos=new Vector3(column*m_OffsetX,(m_BoardRows+1)*m_OffsetY,0.5f);
			}
		}

		// 在中间一列棋盘上方随机生成一个棋子
		GameObject disc=Instantiate((PlayerId==1)?m_DiscBlue:m_DiscRed,new Vector3(spawnPos.x,(m_BoardRows+1)*m_OffsetY,0.5f),Quaternion.identity);		
		return disc;
	}

	//棋子跟随鼠标水平移动
	private void MoveHorizontal()
	{
		Vector3 mousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);		
		if(m_CurrentDisc)
		{
			//限制x的范围在第一个格子和最后一个格子(第7个)之间
			m_CurrentDisc.transform.position=new Vector3(Mathf.Clamp(mousePos.x,0,m_OffsetX*m_BoardRows),(m_BoardRows+1)*m_OffsetY,0.5f);
		}
		//print(m_CurrentDisc.transform.position.x);
	}

	//从上至下 投掷棋子
	private IEnumerator DropDisc(GameObject disc)
	{
		//下落阶段开始
		isFalling=true;

		//colIndex从0开始，范围[0,6]
		int colIndex=Mathf.RoundToInt(disc.transform.position.x/m_OffsetX);

		int rowIndex=FirstFreeRow(colIndex,m_CurrentPlayerId);
		
		//当前列棋子已放满,无可放置棋子的格子
		if(rowIndex==-1)
		{
			//当前列满时, 再次点击不会下落，但是isFalling变成里true，需要在这里改回来
			isFalling=false;			
			yield break;
		}
		
		Vector3 startPos=new Vector3(colIndex*m_OffsetX,disc.transform.position.y,disc.transform.position.z);

		Vector3 endPos=new Vector3(colIndex*m_OffsetX,rowIndex*m_OffsetY,disc.transform.position.z);
		//复制一个当前棋子作为新棋子,以防止Update中MoveHorizontal一直执行使棋子下落后自动返回顶部
		GameObject newDisc=Instantiate(disc) as GameObject;
		//使得当前棋子SpriteRenderer组件失效，即消失
		disc.GetComponent<SpriteRenderer>().enabled=false;
		
		float timer=0;
		while(timer<1)
		{
			timer+=Time.deltaTime;
			newDisc.transform.position=Vector3.Lerp(startPos,endPos,timer);	
			yield return null;
		}		
		
		DestroyImmediate(disc);

		isWin=	CheckForVictory(rowIndex,colIndex);
		if (!isWin)
		{
			//如果还没赢,生成一个对手棋子			
			m_CurrentDisc=AddDisc(3-m_CurrentPlayerId);
		}
		
		//下落阶段结束
		isFalling=false;
		yield return null;
	}

	//计算当前列最下方空格子的行的索引
	private int FirstFreeRow(int colIndex,int playerId)
	{
		//maxRowIndex: 当前列最上方已被玩家棋子占据的行的索引
		int maxRowIndex=-1;
		for (int i = m_BoardRows-1; i >= 0; i--)
		{
			if (m_BoardField[i,colIndex]!=0)
			{
				maxRowIndex=i;
				//print("maxRowIndex:"+maxRowIndex);
				break;
			}
		}
		//判断将要投掷的格子是否已经超出了范围（6行）
		if(maxRowIndex+1!=m_BoardRows)
		{
			//若当前格子已被占据,则将表示棋盘的对应数组值改为玩家id
			//print("playerId"+playerId);
			m_BoardField[maxRowIndex+1,colIndex]=playerId;			
			return maxRowIndex+1;
		}
		return -1;
	}

	//可投掷列
	private List<int> GetPossibleCol()
	{
		List<int> possibleMoves=new List<int>();
		for (int i = 0; i < m_BoardCols; i++)
		{
			//如果每一列的最上方为0,即当前列为可投掷列
			if (m_BoardField[m_BoardRows-1,i]==0)
			{
				possibleMoves.Add(i);
			}
		}
		return possibleMoves;
	}

	//判断格子是否已经超出边界
	private int CellValue(int row,int col)
	{
		//没有超出边界
		if (row>=0&&row<=m_BoardRows-1&&col>=0&&col<=m_BoardCols-1)
		{
			return m_BoardField[row,col];	
		}		
		return -1;
	}

	//查找指定方向的棋子的数量 row/col 行和列  row_inc行上的增量  col_inc列上的增量
	private int GetAdj(int row,int col,int row_inc,int col_inc)
	{
		if (CellValue(row,col)==CellValue(row+row_inc,col+col_inc)&&CellValue(row,col)!=-1&&CellValue(row+row_inc,col+col_inc)!=-1
		&&CellValue(row,col)!=0&&CellValue(row+row_inc,col+col_inc)!=0)
		{
			return 1+GetAdj(row+row_inc,col+col_inc,row_inc,col_inc);
		}
		else
		{
			return 0;
		}
	}

	//判断是否胜利
	private bool CheckForVictory(int row,int col)
	{
		//水平方向
		if (GetAdj(row,col,0,-1)+GetAdj(row,col,0,1)>2)
		{
			return true;
		}
		else
		{
			//垂直方向
			if(GetAdj(row,col,-1,0)>2)
			{
				return true;
			}
			else
			{
				//45度
				if(GetAdj(row,col,-1,-1)+GetAdj(row,col,1,1)>2)
				{
					return true;
				}
				else
				{
					//135度
					if(GetAdj(row,col,-1,1)+GetAdj(row,col,1,-1)>2)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
		}

	}

	//人工智能,使棋子下落更智能
	private List<int> AI()
	{
		//获取可投掷棋子的列的集合
		List<int> possibleMoves=GetPossibleCol();
		//最佳的投掷列的集合   最佳投掷列:连线的同色棋子数量最多
		List<int> AIMoves=new List<int>();
		//连线的同色棋子数
		int blocked;
		int bestBlocked=0;
		for (int i = 0; i < possibleMoves.Count; i++)
		{
			//当前列第一个空闲格子的行的索引为currentRow+1			
			int currentRow=-1;
			//遍历所有行数
			for (int j = 5; j >= 0; j--)
			{
				if (m_BoardField[j,possibleMoves[i]]!=0)
				{
					currentRow=j;
					break;
				}
			}
			if (currentRow+1!=6)
			{
				//假设在当前棋子上方放置一个棋子,记录其在水平方向同色棋子数的个数
				m_BoardField[currentRow+1,possibleMoves[i]]=1;				 
				blocked=GetAdj(currentRow+1,possibleMoves[i],0,1)+GetAdj(currentRow+1,possibleMoves[i],0,-1 );
				//判断水平方向和竖直方向同色棋子数的最大值
				blocked=Mathf.Max(blocked,GetAdj(currentRow+1,possibleMoves[i],1,0)+GetAdj(currentRow+1,possibleMoves[i],-1,0));
				//判断上述最大值和两斜线同色棋子数最大值
				blocked=Mathf.Max(blocked,GetAdj(currentRow+1,possibleMoves[i],-1,1)+GetAdj(currentRow+1,possibleMoves[i],1,-1));
				blocked=Mathf.Max(blocked,GetAdj(currentRow+1,possibleMoves[i],1,1)+GetAdj(currentRow+1,possibleMoves[i],-1,-1));
				if(blocked>=bestBlocked)
				{
					if(blocked>bestBlocked)
					{
						bestBlocked=blocked;
						AIMoves=new List<int>();
					}
					AIMoves.Add(possibleMoves[i]);					
				}
			}
			m_BoardField[currentRow+1,possibleMoves[i]]=0;			
		}
		return AIMoves;
	}
}