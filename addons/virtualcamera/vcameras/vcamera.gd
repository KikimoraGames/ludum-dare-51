tool
extends Node2D

class_name VCamera, "res://addons/virtualcamera/vcameras/vcamera.svg"

export(int, 0, 1024) var priority : int = 10
export var enabled : bool = true

export var transition_time : float = 1.0
export(float, EASE) var transition_ease : float = -2.0

func _ready():
	add_to_group("vcamera", true)
