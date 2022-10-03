extends UserInput
class_name LookAhead


export var look_ahead_distance : float = 2
export var spring_constant_look_ahead: float = 75
export var spring_constant_return: float = 75


var current_velocity: Vector2 = Vector2.ZERO


func _ready():
	process_priority = 1000


func _process(delta : float):
	# delta = delta / Engine.time_scale
	var input_velocity := Input.get_vector("move_left", "move_right", "look_up", "look_down")
	var target = Vector2(input_velocity.x, input_velocity.y).normalized() * look_ahead_distance
	var current_position = position
	var spring_constant: float = 0
	
	if input_velocity.length_squared() > 0.01:
		spring_constant = spring_constant_look_ahead
	else:
		spring_constant = spring_constant_return
		target = Vector2.ZERO

	var xChange = Util.critically_dampened_spring(spring_constant, target.x, current_position.x, current_velocity.x, delta)
	var yChange = Util.critically_dampened_spring(spring_constant, target.y, current_position.y, current_velocity.y, delta)

	current_velocity = Vector2(xChange[1], yChange[1])
	var new_position := Vector2(xChange[0], yChange[0])
	position = new_position
	
