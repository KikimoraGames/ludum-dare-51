tool
extends TransformModifier
class_name FollowDampened


export var target_path : NodePath
export var spring_constant: float = 75


var target: Node2D
var current_velocity: Vector2 = Vector2.ZERO

func _ready():
	process_priority = 1000


func _process(delta : float):
	if target == null || not is_instance_valid(target):
		target = get_node(target_path)
	
	if not is_instance_valid(target):
		return
	
	delta = delta / Engine.time_scale
	var target_position := target.global_position
	var current_position := global_position
	
	var distance := target_position.distance_to(current_position)
	
	var xChange = Util.critically_dampened_spring(spring_constant,target_position.x, current_position.x, current_velocity.x, delta)
	var yChange = Util.critically_dampened_spring(spring_constant, target_position.y, current_position.y, current_velocity.y, delta)

	current_velocity = Vector2(xChange[1], yChange[1])
	var new_position := Vector2(xChange[0], yChange[0])
	global_position = new_position
	

func snap_to_target() -> void:
	global_position = target.global_position
