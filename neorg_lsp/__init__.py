import logging
import re
from sys import stdin


from neorg_lsp.methods.factory import MethodHandlerFactory
from neorg_lsp.rpc import IncomingMessage
from neorg_lsp.state import State

logging.basicConfig(filename="wow.log", level=logging.DEBUG)
log = logging.getLogger("neorg-lsp")


# TODO: Implement State
# TODO: Implement textOpen and textEdit to populate references and categories
lspState = State()

if __name__ == "__main__":
    buffer = b""
    while stdin.readable():
        buffer = buffer + stdin.buffer.read(1)

        # TODO: Smarter validation of input and error handling for proper reset
        # I need some sort of scanner to find Content-Length and discard everything before it.
        match_header = re.match(
            r"Content-Length: (\d+)\r\n\r\n", buffer.decode("utf-8")
        )
        if not match_header:
            continue
        buffer = b""
        content_length = int(match_header.group(1))
        content = stdin.buffer.read(content_length)
        base = IncomingMessage.decode(content)

        handler = MethodHandlerFactory(lspState).create_handler(base.method, content)
        handler.handle_request()
