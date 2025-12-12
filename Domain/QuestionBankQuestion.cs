namespace ExamApp.Domain;

public record QuestionBankQuestion(
    int Id,
    string Text,
    string Category,
    Dictionary<int, string> Answers,
    string? AttachmentImg);
