extends Reference
class_name SafeFrameTimer


class FrameTimer extends Node:
    signal timeout

    var frames := 1

    func _ready() -> void:
# warning-ignore:return_value_discarded
        get_tree().connect("idle_frame", self, "_on_timeout")

    func _on_timeout() -> void:
        frames -= 1
        if frames <= 0:
            emit_signal("timeout")
            queue_free()

static func idle_frame(node:Node, frames:int = 1) -> FrameTimer :
    var t := FrameTimer.new()
    t.frames = frames
    node.add_child(t)
    return t
