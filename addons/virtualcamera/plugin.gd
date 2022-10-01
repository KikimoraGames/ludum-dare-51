tool
extends EditorPlugin


func handles(object: Object) -> bool:
	return object is VCamera
