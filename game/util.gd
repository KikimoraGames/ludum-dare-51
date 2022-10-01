class_name Util
extends Reference

# Game Programming Gems 4, Chapter 1.10
static func critically_dampened_spring(spring_constant: float, target: float, current: float, velocity: float, delta: float ) -> Array:
	var currentToTarget:= target - current
	var springForce:= currentToTarget * spring_constant
	var dampingForce:= -velocity * 2 * sqrt( spring_constant )
	var force:= springForce + dampingForce
	velocity += force * delta
	var displacement:= velocity * delta
	return [current + displacement, velocity]