import enum


class LspMethods(enum.Enum):
    INITIALIZE: str = "initialize"
    SHUTDOWN: str = "shutdown"
    EXIT: str = "exit"
