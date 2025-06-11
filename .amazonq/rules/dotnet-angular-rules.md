# .NET Core WebAPI and Angular Application Rules

Rules for developing with .NET Core WebAPI backend and Angular frontend

## Project Structure

Guidelines for project organization:

- All Angular related files and folders should reside under `src/client` directory
- All server related files should be part of the `src/backend/nmf2wav.api` project
- All scripts should be under `src/scripts` with the same client/backend division

## Backend Development

Guidelines for .NET Core WebAPI development:

- Follow RESTful API design principles for all endpoints
- Controllers should act as endpoints only with minimal logic
- Implement all business logic in the `src/backend/nmf2wav` folder
- Use async/await pattern for all I/O operations
- Implement proper dependency injection using built-in .NET Core DI container
- Use Entity Framework Core for database operations
- Implement proper error handling with middleware
- Return appropriate HTTP status codes
- Use DTOs for data transfer between layers
- Implement input validation using DataAnnotations or FluentValidation
- Follow repository pattern for data access
- Use AutoMapper for object mapping between entities and DTOs

## Frontend Development

Guidelines for Angular development:

- Follow Angular style guide for component structure
- Use Angular services for API communication
- Implement proper state management
- Use Angular reactive forms for complex form handling
- Follow component-based architecture
- Implement lazy loading for feature modules
- Use Angular Material or Bootstrap for UI components
- Follow proper TypeScript practices
- Implement proper error handling for HTTP requests
- Use environment configuration for different deployment environments

## API Communication

Guidelines for communication between frontend and backend:

- Use HttpClient in Angular for API calls
- Implement proper CORS configuration in the backend
- Use JWT for authentication
- Implement proper error handling for API responses
- Use interfaces in TypeScript that match backend DTOs
- Implement request/response interceptors for common operations
- Use environment variables for API URLs
- Implement proper loading states for API calls
- Use RxJS operators for handling API responses
- Implement proper retry mechanisms for failed requests

## Security

Security guidelines for the application:

- Implement proper authentication and authorization
- Use HTTPS for all communications
- Implement proper input validation on both client and server
- Protect against XSS attacks
- Protect against CSRF attacks
- Use secure cookies
- Implement proper password hashing
- Use parameterized queries to prevent SQL injection
- Implement rate limiting for API endpoints
- Follow OWASP security guidelines

## Testing

Testing guidelines for the application:

- Write unit tests for backend services and controllers
- Write integration tests for API endpoints
- Use xUnit or NUnit for .NET testing
- Use Jasmine and Karma for Angular testing
- Implement end-to-end testing with Cypress or Protractor
- Use mocking frameworks like Moq for .NET and jasmine-mock for Angular
- Aim for high test coverage
- Implement CI/CD pipeline with automated testing
- Use code coverage tools
- Write testable code with proper separation of concerns

## Performance

Performance guidelines for the application:

- Implement proper caching strategies
- Use pagination for large data sets
- Optimize database queries
- Implement proper indexing for database tables
- Use compression for API responses
- Optimize Angular bundle size
- Implement lazy loading for Angular modules
- Use server-side rendering when appropriate
- Optimize images and static assets
- Implement proper logging without impacting performance

## Clean Code Principles

Guidelines for writing clean, maintainable code:

- Follow Single Responsibility Principle (SRP) - each class should have only one reason to change
- Follow Open/Closed Principle (OCP) - classes should be open for extension but closed for modification
- Follow Liskov Substitution Principle (LSP) - derived classes must be substitutable for their base classes
- Follow Interface Segregation Principle (ISP) - clients should not be forced to depend on interfaces they don't use
- Follow Dependency Inversion Principle (DIP) - depend on abstractions, not concretions
- Keep methods small and focused on a single task
- Limit method parameters (aim for 3 or fewer)
- Avoid deep nesting of control structures
- Use meaningful names for variables, methods, and classes
- Write self-documenting code with clear intent
- Add comments only when necessary to explain "why" not "what"
- Use proper XML documentation for public APIs
- Extract complex logic into helper methods or classes
- Avoid code duplication (DRY principle)
- Avoid premature optimization
- Refactor regularly to improve code quality

## Code Quality

Code quality guidelines for the application:

- Follow C# coding conventions
- Follow Angular style guide
- Use linting tools like ESLint for TypeScript and StyleCop for C#
- Implement code reviews
- Use meaningful variable and function names
- Write clear and concise comments
- Keep methods small and focused
- Follow SOLID principles
- Use design patterns appropriately
- Refactor code regularly