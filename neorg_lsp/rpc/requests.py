import attrs


@attrs.define
class InitializeRequest:
    capabilities: dict
    processId: int | None = None
    rootUri: str | None = None


