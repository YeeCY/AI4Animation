using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SIGGRAPH_2018;

public class MotionRecorder : MonoBehaviour {
    private bool Recording;
    private string OutputFileName;

    private Vector3 OriginPosition;

    private List<Vector3[]> FramePositions;
    private List<Vector3[]> OutputFramePositions;
    private List<float> TimeStamps;

    private System.DateTime StartTimeStamp;

    public GameObject Panel;

    public GameObject OutputFileNameInputField;

    public Button StartButton;

    public Button StopButton;
    public int Framerate = 60;
    
    private static string Separator = ",\t";
	private static string Accuracy = "F5";

    void OnEnable() {
        Panel.SetActive(true);
        gameObject.GetComponent<BioAnimation_Wolf>().Pause = true;
    }

    void OnDisable() {
        Panel.SetActive(false);
        gameObject.GetComponent<BioAnimation_Wolf>().Pause = false;
    }

    void Start() {
        // FramePositions = new List<Vector3[]>();
        // TimeStamps = new List<float>();
        // OutputFramePositions = new List<Vector3[]>();
        
        Recording = false;
        ApplyGUI();
    }

    void Update() {
        if (Recording) {
            // Debug.Log("I am attached to " + gameObject.name);
            // Debug.Log("I can see " + gameObject.GetComponent<BioAnimation_Wolf>());
            
            Vector3[] Positions = gameObject.GetComponent<BioAnimation_Wolf>().Positions;
            // (chongyiz): Create a deep copy of positions
            Vector3[] PositionsCopy = new Vector3[Positions.Length];
            for (int k = 0; k < Positions.Length; k++) {
                Vector3 position = new Vector3(Positions[k].x, Positions[k].y, Positions[k].z);
                PositionsCopy[k] = position;
            }

            if (FramePositions.Count == 0) {
                OriginPosition.x = PositionsCopy[0].x;
                OriginPosition.y = PositionsCopy[0].y;
                OriginPosition.z = PositionsCopy[0].z;
            }

            // System.DateTime TimeStamp = Utility.GetTimestamp();

            // Debug.Log("TimeScale MotionRecorder Update = " + Time.timeScale);

            // Debug.Log("Length of positions: " + Positions.Length);
            // Debug.Log("Position[0]: " + Positions[0]);
            
            FramePositions.Add(PositionsCopy);
            // Debug.Log("FramePositions[0][0]: " + FramePositions[0][0]);
            // Debug.Log("FramePositions[-1][0]: " + FramePositions[FramePositions.Count - 1][0]);
            // if (TimeStamps.Count == 0) {
            //     TimeStamps.Add(0);
            // } else {
            //     Utility.GetTimestamp();
            // }
            TimeStamps.Add((float)Utility.GetElapsedTime(StartTimeStamp));
            // Debug.Log("TimeStamps: " + TimeStamps[TimeStamps.Count - 1]);
            
            // Debug.Log("Number of frames: " + FramePositions.Count);
            
            // for (int i = 0; i < Positions.Length; i++) {
            //     Debug
            // }
            // for(int i=0; i< Positions.Length; i++) {  // (chongyiz): 27 bones
			// 		Vector3 position = Actor.Bones[i].Transform.position;
			// 		position.y = Positions[i].y;
			// 		Positions[i] = Vector3.Lerp(Positions[i], position, MotionEditing.GetStability());
			// 		// Debug.Log("Bone " + i + " position: x = " + Positions[i][0] + 
			// 		// 		  ", y = " + Positions[i][1] + ", z = " + Positions[i][2]);
			// 		// Debug.Log("Bone " + i + " position: " + Positions[i]);
			// 	}
            // gameObject.GetComponent<BioAnimation_Wolf>().Positions
        }
    }

    public void OnStart() {
        OutputFileName = OutputFileNameInputField.GetComponent<InputField>().text;
        // Debug.Log("Ouput File Name: MotionRecordings/" + OutputFileName);
        FramePositions = new List<Vector3[]>();
        TimeStamps = new List<float>();
        OutputFramePositions = new List<Vector3[]>();
        StartTimeStamp = Utility.GetTimestamp();

        gameObject.GetComponent<BioAnimation_Wolf>().Pause = false;
        OutputFileNameInputField.GetComponent<InputField>().interactable = false;
        Recording = true;
        ApplyGUI();
    }

    public void OnStop() {
        // Debug.Log("OnStop");
        // Debug.Log("Folder: " + Application.dataPath + "/../../MotionRecordings/");


        if (Recording) {
            Data OutputData = new Data(CreateFile(OutputFileName), CreateFile(OutputFileName + "_labels"));

            // Normalize timestamps
            float AnchorTime = TimeStamps[0];
            for(int i = 0; i < TimeStamps.Count; i++) {
                TimeStamps[i] -= AnchorTime;
            }

            // TODO (chongyiz): Implement this
            // Generating
            Debug.Log("Generating Output Frames...");

            float start = TimeStamps[0];
            float end = TimeStamps[TimeStamps.Count - 1];
            // Debug.Log("Start: " + start);
            // Debug.Log("End: " + end);
            // Debug.Log("FramePositions Count: " + FramePositions.Count);
            for(float t = start; t <= end; t += 1f / Framerate) {
                // Editor.LoadFrame(t);
                // states.Add(new State(Editor));
                Vector3[] Positions = GetFramePositions(t);

                // int TotalFrame = FramePositions.Count;
                // Debug.Log("FramePositions Index: " + Mathf.Min(Mathf.RoundToInt(t * Framerate) + 1, TotalFrame));
                // Debug.Log("timestamp " + t);
                // Debug.Log("Positions[0]: " + Positions[0]);

                for(int k = 0; k < Positions.Length; k++) {
                    Vector3 position = Positions[k];
                    // minus orgin offset
                    position.x = position.x - OriginPosition.x;
                    position.z = position.z - OriginPosition.z;
                    // Debug.Log("Normalized position: " + position);

                    OutputData.Feed(position.x, "Bone"+(k+1)+"PositionX");
                    OutputData.Feed(position.y, "Bone"+(k+1)+"PositionY");
                    OutputData.Feed(position.z, "Bone"+(k+1)+"PositionZ");
                }
                OutputData.Store();
            }
            OutputData.Finish();

            gameObject.GetComponent<BioAnimation_Wolf>().Pause = true;
            OutputFileNameInputField.GetComponent<InputField>().interactable = true;
            Recording = false;
            ApplyGUI();
        }
    }
    
    private void ApplyGUI() {
        if(Recording) {
            StartButton.GetComponent<Image>().color = UltiDraw.Mustard;
            StopButton.GetComponent<Image>().color = UltiDraw.BlackGrey;
        } else {
            StartButton.GetComponent<Image>().color = UltiDraw.BlackGrey;
            StopButton.GetComponent<Image>().color = UltiDraw.Mustard;
        }
    }

    // public Frame GetFrame(int index) {
	// 	if(index < 1 || index > GetTotalFrames()) {
	// 		Debug.Log("Please specify an index between 1 and " + GetTotalFrames() + ".");
	// 		return null;
	// 	}
	// 	return Frames[index-1];
	// }

	public Vector3[] GetFramePositions(float time) {
        int TotalFrame = FramePositions.Count;
        float TotalTime = (float)FramePositions.Count / Framerate;
		if(time < 0f || time > TotalTime) {
			Debug.Log("Please specify a time between 0 and " + TotalTime + ".");
			return null;
		}
		return FramePositions[Mathf.Min(Mathf.RoundToInt(time * Framerate) + 1, TotalFrame)];
	}

    private StreamWriter CreateFile(string name) {
		string filename = string.Empty;
		string folder = Application.dataPath + "/../../MotionRecordings/";
		if(!File.Exists(folder + name + ".txt")) {
			filename = folder + name;
		} else {
			int i = 1;
			while(File.Exists(folder + name + " (" + i + ").txt")) {
				i += 1;
			}
			filename = folder + name + " (" + i + ")";
		}
		return File.CreateText(filename + ".txt");
	}

    // (chongyiz): copy from MotionEdictor
    public class Data {
		public StreamWriter File, Labels;
		// public enum ID {Standard, Ignore, IgnoreMean, IgnoreStd}

		// public RunningStatistics[] Mean = null;
		// public RunningStatistics[] Std = null;

		private float[] Values = new float[0];
		// private ID[] Types = new ID[0];
		private string[] Names = new string[0];
		// private float[] Weights = new float[0];
		private int Dim = 0;

		public Data(StreamWriter file, StreamWriter labels) {
			File = file;
			// Norm = norm;
			Labels = labels;
		}

		public void Feed(float value, string name) {
			Dim += 1;
			if(Values.Length < Dim) {
				ArrayExtensions.Add(ref Values, value);
			} else {
				Values[Dim-1] = value;
			}
			// if(Types.Length < Dim) {
			// 	ArrayExtensions.Add(ref Types, type);
			// }
			if(Names.Length < Dim) {
				ArrayExtensions.Add(ref Names, name);
			}
			// if(Weights.Length < Dim) {
			// 	ArrayExtensions.Add(ref Weights, weight);
			// }
		}

		public void Feed(float[] values, string name) {
			for(int i = 0; i < values.Length; i++) {
				Feed(values[i], name + (i+1));
			}
		}

		public void Store() {
			// if(Norm != null) {
			// 	if(Mean == null && Std == null) {
			// 		Mean = new RunningStatistics[Values.Length];
			// 		for(int i=0; i<Mean.Length; i++) {
			// 			Mean[i] = new RunningStatistics();
			// 		}
			// 		Std = new RunningStatistics[Values.Length];
			// 		for(int i=0; i<Std.Length; i++) {
			// 			Std[i] = new RunningStatistics();
			// 		}
			// 	}
			// 	for(int i=0; i<Values.Length; i++) {
			// 		switch(Types[i]) {
			// 			case ID.Standard:		//Ground Truth
			// 			Mean[i].Add(Values[i]);
			// 			Std[i].Add(Values[i]);
			// 			break;
			// 			case ID.Ignore:			//Mean 0.0 Std 1.0
			// 			Mean[i].Add(0f);
			// 			Std[i].Add(-1f);
			// 			Std[i].Add(1f);
			// 			break;
			// 			case ID.IgnoreMean:		//Mean 0.0 Std GT
			// 			Mean[i].Add(0f);
			// 			Std[i].Add(Values[i]);
			// 			break;
			// 			case ID.IgnoreStd:		//Mean GT Std 1.0
			// 			Mean[i].Add(Values[i]);
			// 			Std[i].Add(-1f);
			// 			Std[i].Add(1f);
			// 			break;
			// 		}
			// 	}
			// }

			if(File != null) {
				string line = string.Empty;
				for(int i=0; i<Values.Length; i++) {
					line += Values[i].ToString(Accuracy) + Separator;
				}
				line = line.Remove(line.Length-1);// remove '\t'
				line = line.Remove(line.Length-1);// remove ','
				// line = line.Replace(",",".");
				File.WriteLine(line);
			}

			Dim = 0;
		}

		public void Finish() {
			if(Labels != null) {
				for(int i=0; i< Names.Length; i++) {
					Labels.WriteLine("[" + i + "]" + " " + Names[i]);
				}
				Labels.Close();
			}

			if(File != null) {
				File.Close();
			}

			// if(Norm != null) {
			// 	string mean = string.Empty;
			// 	for(int i=0; i<Mean.Length; i++) {
			// 		mean += Mean[i].Mean().ToString(Accuracy) + Separator;
			// 	}
			// 	mean = mean.Remove(mean.Length-1);
			// 	mean = mean.Replace(",",".");
			// 	Norm.WriteLine(mean);

			// 	string std = string.Empty;
			// 	for(int i=0; i<Std.Length; i++) {
			// 		std += (Std[i].Std() / Weights[i]).ToString(Accuracy) + Separator;
			// 	}
			// 	std = std.Remove(std.Length-1);
			// 	std = std.Replace(",",".");
			// 	Norm.WriteLine(std);

			// 	Norm.Close();
			// }
		}
	}

    // public class State {
	// 	public Matrix4x4 Root;
	// 	public Matrix4x4[] Posture;
	// 	public Vector3[] Velocities;
	// 	public Trajectory Trajectory;

	// 	public State(MotionEditor editor) {
	// 		MotionEditor.File file = editor.GetCurrentFile();
	// 		Frame frame = editor.GetCurrentFrame();

	// 		Root = editor.GetActor().GetRoot().GetWorldMatrix();
	// 		Posture = editor.GetActor().GetPosture();
	// 		Velocities = editor.GetActor().GetVelocities();
	// 		Trajectory = ((TrajectoryModule)file.Data.GetModule(Module.TYPE.Trajectory)).GetTrajectory(frame, editor.Mirror);
	// 	}
	// }

    // void Awake() {

    // }

    // void Start() {
        
    // }

    // void OnEnable() {
    // 	Awake();
    // 	Start();
    // }

    // void Update() {
    // 	for(int i=0; i<Positions.Length; i++) {
    // 		while(Positions[i].Count >= Frames) {
    // 			Positions[i].RemoveAt(0);
    // 		}
    // 		Positions[i].Add(Feet[i].position);
    // 		Elements += 1;
    // 	}
    // }

    // void OnRenderObject() {
    // 	if(Elements <= Feet.Length) {
    // 		return;
    // 	}

    // 	UltiDraw.Begin();

    // 	Color[] colors = UltiDraw.GetRainbowColors(Feet.Length);
    // 	for(int i=0; i<colors.Length; i++) {
    // 		colors[i] = colors[i].Darken(0.25f);
    // 	}
    // 	for(int i=0; i<Feet.Length; i++) {
    // 		UltiDraw.DrawSphere(Feet[i].transform.position, Quaternion.identity, 0.075f, colors[i]);
    // 	}

    // 	float border = 0.01f;
    // 	float width = Rect.W;
    // 	float height = Feet.Length * Rect.H + (Feet.Length-1) * border/2f; 

    // 	UltiDraw.DrawGUIRectangle(Rect.GetPosition(), new Vector2(width, height), UltiDraw.DarkGrey.Transparent(0.75f), 0.5f*border, UltiDraw.BlackGrey);
    // 	float pivot = 0.5f * height;
    // 	for(int i=1; i<=Feet.Length; i++) {
    // 		pivot -= Rect.H/2f;
    // 		UltiDraw.DrawGUIRectangle(Rect.GetPosition() + new Vector2(0f, pivot), new Vector2(Rect.W, Rect.H), UltiDraw.White.Transparent(0.5f));
    // 		for(int j=0; j<Positions[i-1].Count; j++) {
    // 			float p = (float)j/(float)(Positions[i-1].Count-1);
    // 			p = Utility.Normalise(p, 0f, 1f, 0.5f*Thickness/Rect.W, (Rect.W-0.5f*Thickness)/Rect.W);
    // 			float x = Rect.X - 0.5f*Rect.W + p*Rect.W;
    // 			float yTop = pivot + Rect.H/2f;
    // 			float yBot = pivot - Rect.H/2f;
    // 			float h = Positions[i-1][j].y - Utility.GetHeight(Positions[i-1][j], LayerMask.GetMask("Ground"));
    // 			if(h < Thresholds[i-1]) {
    // 				UltiDraw.DrawGUILine(new Vector2(x, Rect.Y + yTop), new Vector2(x, Rect.Y + yBot), Thickness, colors[i-1]);
    // 			}
    // 		}
    // 		pivot -= border/2f;
    // 		pivot -= Rect.H/2f;
    // 	}

    // 	UltiDraw.End();
    // }
}
