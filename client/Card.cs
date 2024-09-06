using shared;
using Godot;

public partial class Card : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	Material clayMat;
	[Export]
	Material woodMat;
	[Export]
	Material ironMat;
	[Export]
	Material sheepMat;


	[Export]
	MeshInstance3D mesh;

	public Player player;
	bool isSelected = false;

	string type;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void setPlayer(Player p){
		player = p;
	}

	public void SetTypeRc(RessourceCard rc){
		switch(rc){
			case RessourceCard.Clay:
				mesh.SetSurfaceOverrideMaterial(0,clayMat);
				type = "Clay";
				break;
			case RessourceCard.Wood:
				mesh.SetSurfaceOverrideMaterial(0,woodMat);
				type = "Wood";
				break;
			case RessourceCard.Sheep:
				mesh.SetSurfaceOverrideMaterial(0,sheepMat);
				type = "Sheep";
				break;
			case RessourceCard.Iron:
				mesh.SetSurfaceOverrideMaterial(0,ironMat);
				type = "Iron";
				break;
		}
		
	}

	public void Select(){
		var a = mesh.GetSurfaceOverrideMaterial(0).NextPass as ShaderMaterial;
		//Console.WriteLine(a.GetShaderParameter("outline_width").As<float>());
		isSelected = !isSelected;
		if(!isSelected)
			a.SetShaderParameter("outline_width",1.0f);
		else
			a.SetShaderParameter("outline_width",3.0f);
	}
}
