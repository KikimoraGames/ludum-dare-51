tool
extends "res://addons/virtualcamera/transform_modifiers/transform_modifier.gd"

class_name ShakeImpulse

export var noise : OpenSimplexNoise
export var speed : float = 1.0

export var translation_strength : Vector2 = Vector2.ZERO
export var rotation_strength : float = 0

export var duration : float = 0.0 setget set_duration
func set_duration(value: float) -> void:
	duration = value
	current_duration = value

var current_duration : float = 0.0
var time: float = 0.0

func _ready():
	if not noise:
		noise = OpenSimplexNoise.new()
		noise.seed = get_instance_id()
		noise.octaves = 4
		noise.persistence = 0.8

const DEG2RAD = TAU / 360

func _process(delta : float):
	if not noise:
		return
	if current_duration <= 0:
		return

	delta = delta / Engine.time_scale
	time += delta * speed * 100
	
	if not Engine.editor_hint:
		current_duration -= delta
		if current_duration <= 0:
			position = Vector2.ZERO
			rotation = 0
			set_process(false)
			return

	position = Vector2(noise.get_noise_1d(time), noise.get_noise_1d(time - 10000))
	position *= translation_strength
	
	rotation = noise.get_noise_1d(time - 30000)
	rotation *= rotation_strength * DEG2RAD


func on_camera_impulse(velocity: Vector2, duration: float) -> void:
	translation_strength = velocity
	time = 0
	self.duration = duration
	set_process(true)

