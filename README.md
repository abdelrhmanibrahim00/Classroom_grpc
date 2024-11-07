Classroom Simulation using gRPC

A C# project that simulates a classroom environment where teachers and doors (representing student entries) interact with a server to manage and monitor a class session. The project leverages gRPC for communication between clients (TeacherClient, DoorClient) and the server (ClassroomService).
Project Structure
Server

The server hosts the ClassroomService, which handles:

    Student Count Management: Accumulates student counts sent by DoorClient and checks if they meet the threshold to start a class.
    Session State: Manages whether the class session is in progress or not.
    Teacher Votes: Processes voting actions from TeacherClient to decide whether to start or end the class.

Clients

The project has two clients that interact with the server:

    DoorClient:
        Sends the count of students entering the classroom.
        Notifies the server whether the door is open or closed.
        Checks the class session state to determine if the door should continue sending students.

    TeacherClient:
        Represents individual teachers who vote to start or end the class based on the server's student count.
        Interacts with the server to receive unique IDs and monitor the class session state.

gRPC Protocol Buffers

The communication between clients and the server is structured using Protocol Buffers (proto) with defined messages and services:

    Messages:
        Teacher, Door, UniqueIdResponse, ClassSessionStateResponse, VoteRequest, etc.
    Services:
        The main service Classroom which defines methods for handling student counts, session states, and teacher votes.

Installation and Setup

    Clone the repository:

    git clone https://github.com/yourusername/ClassroomSolution_grpc.git
    cd ClassroomSolution_grpc

    Build the solution in Visual Studio or your preferred .NET IDE.

    Start the server:
        Run the server application (Classroom) from Visual Studio or the command line.

    Run the clients:
        Run TeacherClient and DoorClient separately. Ensure both can connect to the server using the defined gRPC endpoints.

Configuration

    gRPC Endpoint: The server listens on http://127.0.0.1:5000.
    Ensure the endpoint is consistent across the server and clients.

Troubleshooting

    Connection Errors: Check that the server is running and reachable at the specified address before starting the clients.
    Session State Issues: Ensure proper sequence between student count and voting, as TeacherClient will only attempt to start a session if enough students are registered.

Future Improvements

    Add real-time logging or a UI to visualize client-server interactions.
    Enhance voting thresholds and class session logic for scalability.

License

MIT License
