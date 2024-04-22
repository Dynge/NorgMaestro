import attrs


class Message:
    jsonrpc: str = "2.0"


@attrs.define
class ClientMessage(Message):
    method: str


@attrs.define
class BaseRequest(ClientMessage):
    id: int
    params: dict | None = None


@attrs.define
class BaseNotification(ClientMessage):
    params: dict | None = None


@attrs.define
class BaseResponse(Message):
    id: int
    result: dict | None
    error: dict | None
