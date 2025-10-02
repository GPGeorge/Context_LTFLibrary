# LTF Library - Publication Editing Feature Implementation Plan

## Overview
This document outlines the implementation plan for the publication editing feature, including 5 new components to handle the complex relationships between Publications, Publishers, Categories, and Authors.

## Architecture Overview

```
Admin.razor (Enhanced)
├── Publication Edit Tab
│   ├── Search & Select Publication
│   └── Edit Publication Modal/Page
│       ├── Basic Publication Details
│       ├── Publisher Selection/Management
│       ├── Category Management
│       └── Author/Creator Management
```

## 1. Enhanced Admin.razor - Add Publication Edit Tab

**File:** `Admin.razor` (modify existing)

Add this new navigation link in the admin-nav-links section:

```html
<a href="javascript:void(0)" @onclick="@(() => SetActiveTab("editPubs"))"
   class="admin-nav-link @(ActiveTab == "editPubs" ? "active" : "")">
    Edit Publications
</a>
```

Add this new tab section after the existing user management section:

```html
@if (ActiveTab == "editPubs")
{
    <div class="request-queue">
        <div class="request-queue-header">
            <h3>Publication Management</h3>
            <button @onclick="RefreshEditPubsData" class="btn-base btn-secondary">Refresh</button>
        </div>

        <!-- Search Section -->
        <div class="publication-search-section">
            <div class="search-controls">
                <div class="search-row">
                    <div class="search-field">
                        <label>Title:</label>
                        <input type="text" @bind="PublicationSearchCriteria.Title" @bind:event="oninput"
                               placeholder="Search by title..." />
                    </div>
                    <div class="search-field">
                        <label>Author:</label>
                        <select @bind="PublicationSearchCriteria.CreatorId">
                            <option value="">All Authors</option>
                            @if (Authors != null)
                            {
                                @foreach (var author in Authors)
                                {
                                    <option value="@author.CreatorID">@author.FullName</option>
                                }
                            }
                        </select>
                    </div>
                </div>
                <div class="search-actions">
                    <button @onclick="SearchEditPubsPublications" class="btn-base btn-primary">Search</button>
                    <button @onclick="ClearEditPubsSearch" class="btn-base btn-secondary">Clear</button>
                </div>
            </div>
        </div>

        <!-- Results Table -->
        @if (PublicationSearchResults?.Any() == true)
        {
            <table class="request-table">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Author(s)</th>
                        <th>Year</th>
                        <th>Publisher</th>
                        <th>Type</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var pub in PublicationSearchResults)
                    {
                        <tr>
                            <td class="publication-title">@pub.PublicationTitle</td>
                            <td>@string.Join(", ", pub.Authors)</td>
                            <td>@pub.YearPublished</td>
                            <td>@pub.PublisherName</td>
                            <td>@pub.MediaTypeName</td>
                            <td>
                                <button @onclick="@(() => EditPublication(pub.PublicationID))" 
                                        class="action-btn approve-btn">
                                    Edit
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        }
    </div>
}
```

Add these properties to the @code section:

```csharp
// Publication editing state - for editPubs tab
private PublicationSearchCriteria PublicationSearchCriteria = new();
private List<PublicationSearchResult>? PublicationSearchResults;
private List<CreatorDto>? Authors;
private List<GenreDto>? Genres;
private List<MediaTypeDto>? MediaTypes;
private List<PublisherDto>? Publishers;
private bool ShowEditPublicationModal = false;
private PublicationEditDto? EditingPublication;

private async Task SearchEditPubsPublications()
{
    var searchRequest = new PublicationSearchRequest
    {
        Criteria = PublicationSearchCriteria,
        Page = 1,
        PageSize = 50
    };
    var response = await PublicationService.SearchPublicationsAsync(searchRequest);
    PublicationSearchResults = response.Results;
}

private void ClearEditPubsSearch()
{
    PublicationSearchCriteria = new();
    PublicationSearchResults = null;
}

private async Task RefreshEditPubsData()
{
    await LoadDropdownDataForPublications();
    if (PublicationSearchResults != null)
    {
        await SearchEditPubsPublications();
    }
}

private async Task EditPublication(int publicationId)
{
    EditingPublication = await PublicationService.GetPublicationForEditAsync(publicationId);
    ShowEditPublicationModal = true;
}
```

## 2. Publication Edit Modal/Page

**File:** `PublicationEdit.razor` (new component)

```html
@page "/admin/publication/edit/{id:int}"
@using LTF_Library_V1.DTOs
@using LTF_Library_V1.Services
@using Microsoft.AspNetCore.Authorization
@inject IPublicationService PublicationService
@inject IPublisherService PublisherService
@inject IJSRuntime JSRuntime
@inject NavigationManager Navigation
@attribute [Authorize(Roles = "Admin,Staff")]

<PageTitle>Edit Publication - LTF Library Admin</PageTitle>

@if (Publication != null)
{
    <div class="edit-publication-container">
        <div class="edit-header">
            <h1>Edit Publication</h1>
            <div class="edit-actions">
                <button @onclick="SavePublication" class="btn-base btn-success" disabled="@IsSaving">
                    @(IsSaving ? "Saving..." : "Save Changes")
                </button>
                <button @onclick="CancelEdit" class="btn-base btn-secondary">Cancel</button>
            </div>
        </div>

        <EditForm Model="Publication" OnValidSubmit="SavePublication">
            <DataAnnotationsValidator />
            <ValidationSummary />

            <div class="edit-sections">
                <!-- Basic Information Section -->
                <div class="edit-section">
                    <h3>Basic Information</h3>
                    <div class="form-grid">
                        <div class="form-field full-width">
                            <label for="title">Title *</label>
                            <InputText @bind-Value="Publication.PublicationTitle" id="title" class="large-input" />
                        </div>
                        
                        <div class="form-field">
                            <label for="isbn">ISBN</label>
                            <InputText @bind-Value="Publication.ISBN" id="isbn" />
                        </div>
                        
                        <div class="form-field">
                            <label for="year">Year Published</label>
                            <InputText @bind-Value="Publication.YearPublished" id="year" />
                        </div>
                        
                        <div class="form-field">
                            <label for="edition">Edition</label>
                            <InputText @bind-Value="Publication.Edition" id="edition" />
                        </div>
                        
                        <div class="form-field">
                            <label for="pages">Pages</label>
                            <InputNumber @bind-Value="Publication.Pages" id="pages" />
                        </div>
                        
                        <div class="form-field">
                            <label for="volume">Volume</label>
                            <InputNumber @bind-Value="Publication.Volume" id="volume" />
                        </div>
                        
                        <div class="form-field">
                            <label for="numVolumes">Number of Volumes</label>
                            <InputNumber @bind-Value="Publication.NumberOfVolumes" id="numVolumes" />
                        </div>
                    </div>
                </div>

                <!-- Publisher Section -->
                <div class="edit-section">
                    <h3>Publisher</h3>
                    <div class="publisher-selection">
                        <div class="form-field">
                            <label>Select Publisher</label>
                            <div class="input-with-button">
                                <select @bind="Publication.PublisherID">
                                    <option value="">Select Publisher...</option>
                                    @if (Publishers != null)
                                    {
                                        @foreach (var pub in Publishers)
                                        {
                                            <option value="@pub.PublisherID">@pub.DisplayName</option>
                                        }
                                    }
                                </select>
                                <button type="button" @onclick="ShowPublisherModal" class="btn-base btn-secondary">
                                    Manage Publishers
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Authors Section -->
                <div class="edit-section">
                    <h3>Authors/Creators</h3>
                    <div class="authors-management">
                        <div class="selected-authors">
                            <h4>Selected Authors:</h4>
                            @if (Publication.SelectedAuthors?.Any() == true)
                            {
                                <div class="author-chips">
                                    @foreach (var author in Publication.SelectedAuthors)
                                    {
                                        <span class="author-chip">
                                            @author.FullName
                                            <button type="button" @onclick="@(() => RemoveAuthor(author))" class="remove-btn">×</button>
                                        </span>
                                    }
                                </div>
                            }
                            else
                            {
                                <p class="no-items">No authors selected</p>
                            }
                        </div>
                        <button type="button" @onclick="ShowAuthorModal" class="btn-base btn-primary">
                            Manage Authors
                        </button>
                    </div>
                </div>

                <!-- Categories Section -->
                <div class="edit-section">
                    <h3>Categories</h3>
                    <div class="categories-management">
                        <div class="selected-categories">
                            <h4>Selected Categories:</h4>
                            @if (Publication.SelectedCategories?.Any() == true)
                            {
                                <div class="category-chips">
                                    @foreach (var category in Publication.SelectedCategories)
                                    {
                                        <span class="category-chip">
                                            @category.Genre
                                            <button type="button" @onclick="@(() => RemoveCategory(category))" class="remove-btn">×</button>
                                        </span>
                                    }
                                </div>
                            }
                            else
                            {
                                <p class="no-items">No categories selected</p>
                            }
                        </div>
                        <button type="button" @onclick="ShowCategoryModal" class="btn-base btn-primary">
                            Manage Categories
                        </button>
                    </div>
                </div>

                <!-- Keywords Section -->
                <div class="edit-section">
                    <h3>Keywords</h3>
                    <div class="keywords-management">
                        <div class="selected-keywords">
                            <h4>Selected Keywords:</h4>
                            @if (Publication.Keywords?.Any() == true)
                            {
                                <div class="keyword-chips">
                                    @foreach (var keyword in Publication.Keywords)
                                    {
                                        <span class="keyword-chip">
                                            @keyword
                                            <button type="button" @onclick="@(() => RemoveKeyword(keyword))" class="remove-btn">×</button>
                                        </span>
                                    }
                                </div>
                            }
                            else
                            {
                                <p class="no-items">No keywords selected</p>
                            }
                        </div>
                        <button type="button" @onclick="ShowKeywordModal" class="btn-base btn-primary">
                            Manage Keywords
                        </button>
                    </div>
                </div>

                <!-- Description Section -->
                <div class="edit-section">
                    <h3>Description & Notes</h3>
                    <div class="form-field full-width">
                        <label for="comments">Description</label>
                        <InputTextArea @bind-Value="Publication.Comments" id="comments" rows="5" />
                    </div>
                </div>
            </div>
        </EditForm>
    </div>
}

<!-- Publisher Management Modal -->
@if (ShowPublisherModalFlag)
{
    <PublisherManagementModal @ref="PublisherModal" 
                             OnClose="ClosePublisherModal" 
                             OnPublisherSelected="SelectPublisher" />
}

<!-- Author Management Modal -->
@if (ShowAuthorModalFlag)
{
    <AuthorManagementModal @ref="AuthorModal"
                          SelectedAuthors="Publication?.SelectedAuthors"
                          OnClose="CloseAuthorModal"
                          OnAuthorsUpdated="UpdateAuthors" />
}

<!-- Category Management Modal -->
@if (ShowCategoryModalFlag)
{
    <CategoryManagementModal @ref="CategoryModal"
                            SelectedCategories="Publication?.SelectedCategories"
                            OnClose="CloseCategoryModal"
                            OnCategoriesUpdated="UpdateCategories" />
}

<!-- Keyword Management Modal -->
@if (ShowKeywordModalFlag)
{
    <KeywordManagementModal @ref="KeywordModal"
                           SelectedKeywords="Publication?.Keywords"
                           OnClose="CloseKeywordModal"
                           OnKeywordsUpdated="UpdateKeywords" />
}

@code {
    [Parameter] public int Id { get; set; }
    
    private PublicationEditDto? Publication;
    private List<PublisherDto>? Publishers;
    private bool IsSaving = false;
    
    // Modal state
    private bool ShowPublisherModalFlag = false;
    private bool ShowAuthorModalFlag = false;
    private bool ShowCategoryModalFlag = false;
    private bool ShowKeywordModalFlag = false;
    
    private PublisherManagementModal? PublisherModal;
    private AuthorManagementModal? AuthorModal;
    private CategoryManagementModal? CategoryModal;
    private KeywordManagementModal? KeywordModal;

    protected override async Task OnInitializedAsync()
    {
        await LoadPublication();
        await LoadDropdownData();
    }

    private async Task LoadPublication()
    {
        Publication = await PublicationService.GetPublicationForEditAsync(Id);
    }

    private async Task LoadDropdownData()
    {
        Publishers = await PublisherService.GetPublishersAsync();
    }

    private async Task SavePublication()
    {
        if (Publication == null) return;
        
        IsSaving = true;
        try
        {
            var result = await PublicationService.UpdatePublicationAsync(Publication);
            if (result.Success)
            {
                await JSRuntime.InvokeVoidAsync("alert", "Publication updated successfully!");
                Navigation.NavigateTo("/admin");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Error: {result.Message}");
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void CancelEdit()
    {
        Navigation.NavigateTo("/admin");
    }

    // Publisher modal methods
    private void ShowPublisherModal() => ShowPublisherModalFlag = true;
    private void ClosePublisherModal() => ShowPublisherModalFlag = false;
    private void SelectPublisher(PublisherDto publisher)
    {
        if (Publication != null)
        {
            Publication.PublisherID = publisher.PublisherID;
        }
        ClosePublisherModal();
    }

    // Author modal methods
    private void ShowAuthorModal() => ShowAuthorModalFlag = true;
    private void CloseAuthorModal() => ShowAuthorModalFlag = false;
    private void UpdateAuthors(List<CreatorDto> authors)
    {
        if (Publication != null)
        {
            Publication.SelectedAuthors = authors;
        }
        CloseAuthorModal();
    }
    private void RemoveAuthor(CreatorDto author)
    {
        Publication?.SelectedAuthors?.Remove(author);
    }

    // Category modal methods
    private void ShowCategoryModal() => ShowCategoryModalFlag = true;
    private void CloseCategoryModal() => ShowCategoryModalFlag = false;
    private void UpdateCategories(List<GenreDto> categories)
    {
        if (Publication != null)
        {
            Publication.SelectedCategories = categories;
        }
        CloseCategoryModal();
    }
    private void RemoveCategory(GenreDto category)
    {
        Publication?.SelectedCategories?.Remove(category);
    }

    // Keyword modal methods
    private void ShowKeywordModal() => ShowKeywordModalFlag = true;
    private void CloseKeywordModal() => ShowKeywordModalFlag = false;
    private void UpdateKeywords(List<string> keywords)
    {
        if (Publication != null)
        {
            Publication.Keywords = keywords;
        }
        CloseKeywordModal();
    }
    private void RemoveKeyword(string keyword)
    {
        Publication?.Keywords?.Remove(keyword);
    }
}
```

## 3. Publisher Management Modal

**File:** `PublisherManagementModal.razor` (new component)

```html
@using LTF_Library_V1.DTOs
@using LTF_Library_V1.Services
@inject IPublisherService PublisherService
@inject IJSRuntime JSRuntime

<div class="modal">
    <div class="modal-content large-modal">
        <div class="modal-header">
            <h2>Manage Publishers</h2>
            <button class="close-btn" @onclick="Close">&times;</button>
        </div>

        <div class="modal-body">
            <!-- Add New Publisher -->
            <div class="add-section">
                <h3>Add New Publisher</h3>
                <EditForm Model="NewPublisher" OnValidSubmit="AddPublisher">
                    <div class="add-form">
                        <InputText @bind-Value="NewPublisher.Publisher1" placeholder="Publisher Name" />
                        <InputText @bind-Value="NewPublisher.PublisherGoogle" placeholder="Google Publisher Name (optional)" />
                        <button type="submit" class="btn-base btn-success" disabled="@IsAdding">
                            @(IsAdding ? "Adding..." : "Add")
                        </button>
                    </div>
                </EditForm>
            </div>

            <!-- Publisher List -->
            <div class="list-section">
                <h3>Existing Publishers</h3>
                <div class="search-box">
                    <input type="text" @bind="SearchTerm" @bind:event="oninput" @onkeyup="FilterPublishers" 
                           placeholder="Search publishers..." />
                </div>
                
                <div class="publisher-list">
                    @if (FilteredPublishers?.Any() == true)
                    {
                        @foreach (var publisher in FilteredPublishers)
                        {
                            <div class="publisher-item">
                                @if (EditingPublisherId == publisher.PublisherID)
                                {
                                    <div class="edit-form">
                                        <InputText @bind-Value="EditingPublisher.Publisher1" />
                                        <InputText @bind-Value="EditingPublisher.PublisherGoogle" />
                                        <button @onclick="SavePublisher" class="btn-base btn-success btn-sm">Save</button>
                                        <button @onclick="CancelEdit" class="btn-base btn-secondary btn-sm">Cancel</button>
                                    </div>
                                }
                                else
                                {
                                    <div class="publisher-display">
                                        <span class="publisher-name">@publisher.DisplayName</span>
                                        <div class="publisher-actions">
                                            <button @onclick="@(() => SelectPublisher(publisher))" class="btn-base btn-primary btn-sm">
                                                Select
                                            </button>
                                            <button @onclick="@(() => StartEdit(publisher))" class="btn-base btn-secondary btn-sm">
                                                Edit
                                            </button>
                                            <button @onclick="@(() => DeletePublisher(publisher))" class="btn-base btn-danger btn-sm">
                                                Delete
                                            </button>
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                    }
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<PublisherDto> OnPublisherSelected { get; set; }

    private List<PublisherDto> Publishers = new();
    private List<PublisherDto> FilteredPublishers = new();
    private PublisherDto NewPublisher = new();
    private PublisherDto EditingPublisher = new();
    private int? EditingPublisherId = null;
    private string SearchTerm = "";
    private bool IsAdding = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadPublishers();
    }

    private async Task LoadPublishers()
    {
        Publishers = await PublisherService.GetPublishersAsync();
        FilterPublishers();
    }

    private void FilterPublishers()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            FilteredPublishers = Publishers.ToList();
        }
        else
        {
            FilteredPublishers = Publishers
                .Where(p => p.DisplayName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        StateHasChanged();
    }

    private async Task AddPublisher()
    {
        if (string.IsNullOrWhiteSpace(NewPublisher.Publisher1)) return;

        IsAdding = true;
        try
        {
            var result = await PublisherService.AddPublisherAsync(NewPublisher);
            if (result.Success)
            {
                NewPublisher = new();
                await LoadPublishers();
                await JSRuntime.InvokeVoidAsync("alert", "Publisher added successfully!");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Error: {result.Message}");
            }
        }
        finally
        {
            IsAdding = false;
        }
    }

    private void StartEdit(PublisherDto publisher)
    {
        EditingPublisherId = publisher.PublisherID;
        EditingPublisher = new PublisherDto
        {
            PublisherID = publisher.PublisherID,
            Publisher1 = publisher.Publisher1,
            PublisherGoogle = publisher.PublisherGoogle
        };
    }

    private async Task SavePublisher()
    {
        var result = await PublisherService.UpdatePublisherAsync(EditingPublisher);
        if (result.Success)
        {
            await LoadPublishers();
            CancelEdit();
            await JSRuntime.InvokeVoidAsync("alert", "Publisher updated successfully!");
        }
        else
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error: {result.Message}");
        }
    }

    private void CancelEdit()
    {
        EditingPublisherId = null;
        EditingPublisher = new();
    }

    private async Task DeletePublisher(PublisherDto publisher)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete '{publisher.DisplayName}'?"))
        {
            var result = await PublisherService.DeletePublisherAsync(publisher.PublisherID);
            if (result.Success)
            {
                await LoadPublishers();
                await JSRuntime.InvokeVoidAsync("alert", "Publisher deleted successfully!");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Error: {result.Message}");
            }
        }
    }

    private async Task SelectPublisher(PublisherDto publisher)
    {
        await OnPublisherSelected.InvokeAsync(publisher);
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}
```

## 4. Keyword Management Modal

**File:** `KeywordManagementModal.razor` (new component)

```html
@using LTF_Library_V1.DTOs
@using LTF_Library_V1.Services
@inject IPublicationService PublicationService
@inject IJSRuntime JSRuntime

<div class="modal">
    <div class="modal-content large-modal">
        <div class="modal-header">
            <h2>Manage Keywords</h2>
            <button class="close-btn" @onclick="Close">&times;</button>
        </div>

        <div class="modal-body">
            <!-- Add New Keyword -->
            <div class="add-section">
                <h3>Add Keywords</h3>
                <div class="add-form">
                    <input type="text" @bind="NewKeyword" @onkeypress="HandleKeyPress" 
                           placeholder="Enter keyword and press Enter or click Add" class="keyword-input" />
                    <button @onclick="AddKeyword" class="btn-base btn-success" disabled="@string.IsNullOrWhiteSpace(NewKeyword)">
                        Add
                    </button>
                </div>
                <p class="help-text">Tip: You can enter multiple keywords separated by commas</p>
            </div>

            <!-- Selected Keywords -->
            <div class="selected-section">
                <h3>Selected Keywords for this Publication</h3>
                @if (WorkingKeywords.Any())
                {
                    <div class="keyword-chips editable">
                        @foreach (var keyword in WorkingKeywords)
                        {
                            <span class="keyword-chip">
                                @keyword
                                <button type="button" @onclick="@(() => RemoveKeyword(keyword))" class="remove-btn">×</button>
                            </span>
                        }
                    </div>
                }
                else
                {
                    <p class="no-items">No keywords selected</p>
                }
            </div>

            <!-- Suggested Keywords -->
            <div class="suggestions-section">
                <h3>Suggested Keywords</h3>
                <div class="search-box">
                    <input type="text" @bind="SearchTerm" @bind:event="oninput" @onkeyup="FilterSuggestions" 
                           placeholder="Search existing keywords..." />
                </div>
                
                @if (IsLoadingSuggestions)
                {
                    <p>Loading suggestions...</p>
                }
                else if (FilteredSuggestions.Any())
                {
                    <div class="suggestion-chips">
                        @foreach (var suggestion in FilteredSuggestions.Take(20))
                        {
                            <button type="button" @onclick="@(() => AddExistingKeyword(suggestion))" 
                                    class="suggestion-chip @(WorkingKeywords.Contains(suggestion) ? "selected" : "")"
                                    disabled="@WorkingKeywords.Contains(suggestion)">
                                @suggestion
                            </button>
                        }
                    </div>
                    @if (FilteredSuggestions.Count > 20)
                    {
                        <p class="more-results">And @(FilteredSuggestions.Count - 20) more... (refine search to see them)</p>
                    }
                }
                else if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    <p class="no-results">No existing keywords match your search.</p>
                }
            </div>
        </div>

        <div class="modal-footer">
            <button @onclick="SaveAndClose" class="btn-base btn-success">Save Changes</button>
            <button @onclick="CancelAndClose" class="btn-base btn-secondary">Cancel</button>
        </div>
    </div>
</div>

@code {
    [Parameter] public List<string>? SelectedKeywords { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<List<string>> OnKeywordsUpdated { get; set; }

    private List<string> WorkingKeywords = new();
    private List<string> AllKeywords = new();
    private List<string> FilteredSuggestions = new();
    private string NewKeyword = "";
    private string SearchTerm = "";
    private bool IsLoadingSuggestions = false;

    protected override async Task OnInitializedAsync()
    {
        // Copy the selected keywords to work with
        WorkingKeywords = SelectedKeywords?.ToList() ?? new List<string>();
        await LoadAllKeywords();
        FilterSuggestions();
    }

    private async Task LoadAllKeywords()
    {
        IsLoadingSuggestions = true;
        try
        {
            AllKeywords = await PublicationService.GetAllKeywordsAsync();
        }
        finally
        {
            IsLoadingSuggestions = false;
        }
    }

    private void FilterSuggestions()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            // Show most common keywords not already selected
            FilteredSuggestions = AllKeywords
                .Where(k => !WorkingKeywords.Contains(k, StringComparer.OrdinalIgnoreCase))
                .OrderBy(k => k)
                .ToList();
        }
        else
        {
            FilteredSuggestions = AllKeywords
                .Where(k => k.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) && 
                           !WorkingKeywords.Contains(k, StringComparer.OrdinalIgnoreCase))
                .OrderBy(k => k)
                .ToList();
        }
        StateHasChanged();
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await AddKeyword();
        }
    }

    private async Task AddKeyword()
    {
        if (string.IsNullOrWhiteSpace(NewKeyword)) return;

        // Handle comma-separated keywords
        var keywords = NewKeyword.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();

        foreach (var keyword in keywords)
        {
            if (!WorkingKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
            {
                WorkingKeywords.Add(keyword);
            }
        }

        NewKeyword = "";
        FilterSuggestions(); // Refresh suggestions to remove newly added keywords
        StateHasChanged();
    }

    private void AddExistingKeyword(string keyword)
    {
        if (!WorkingKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
        {
            WorkingKeywords.Add(keyword);
            FilterSuggestions(); // Refresh to remove this keyword from suggestions
            StateHasChanged();
        }
    }

    private void RemoveKeyword(string keyword)
    {
        WorkingKeywords.Remove(keyword);
        FilterSuggestions(); // Refresh to add this keyword back to suggestions
        StateHasChanged();
    }

    private async Task SaveAndClose()
    {
        await OnKeywordsUpdated.InvokeAsync(WorkingKeywords);
    }

    private async Task CancelAndClose()
    {
        await OnClose.InvokeAsync();
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}
```

## 5. Required New DTOs and Service Interfaces

**File:** `PublicationEditDtos.cs` (new file)

```csharp
using System.ComponentModel.DataAnnotations;

namespace LTF_Library_V1.DTOs
{
    public class PublicationEditDto
    {
        public int PublicationID { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string PublicationTitle { get; set; } = string.Empty;
        
        public string? CatalogNumber { get; set; }
        public string? Comments { get; set; }
        public string? CoverPhotoLink { get; set; }
        public string? Edition { get; set; }
        public string? ISBN { get; set; }
        public int? Volume { get; set; }
        public int? NumberOfVolumes { get; set; }
        public int? Pages { get; set; }
        public string? YearPublished { get; set; }
        public int? ConfidenceLevel { get; set; }
        public decimal? ListPrice { get; set; }
        
        // Foreign Keys
        public int? PublisherID { get; set; }
        public int? MediaTypeID { get; set; }
        public int? MediaConditionID { get; set; }
        public int? ShelfID { get; set; }
        
        // Related Collections
        public List<CreatorDto> SelectedAuthors { get; set; } = new();
        public List<GenreDto> SelectedCategories { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
    }

    public class PublisherDto
    {
        public int PublisherID { get; set; }
        public string Publisher1 { get; set; } = string.Empty;
        public string? PublisherGoogle { get; set; }
        public string DisplayName => 
            !string.IsNullOrEmpty(PublisherGoogle) && Publisher1 != PublisherGoogle
                ? $"{Publisher1} ({PublisherGoogle})"
                : Publisher1;
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
```

## 5. Required Service Interface Extensions

**File:** `IPublisherService.cs` (new file)

```csharp
using LTF_Library_V1.DTOs;

namespace LTF_Library_V1.Services
{
    public interface IPublisherService
    {
        Task<List<PublisherDto>> GetPublishersAsync();
        Task<PublisherDto?> GetPublisherAsync(int publisherId);
        Task<ServiceResult> AddPublisherAsync(PublisherDto publisher);
        Task<ServiceResult> UpdatePublisherAsync(PublisherDto publisher);
        Task<ServiceResult> DeletePublisherAsync(int publisherId);
    }
}
```

**Extend IPublicationService.cs:**

```csharp
// Add these methods to the existing interface
Task<PublicationEditDto?> GetPublicationForEditAsync(int publicationId);
Task<ServiceResult> UpdatePublicationAsync(PublicationEditDto publication);
Task<List<string>> GetAllKeywordsAsync();
Task<List<string>> GetKeywordsForPublicationAsync(int publicationId);
```

## 6. Database Considerations

For the many-to-many relationships, you'll need to handle:

### Author/Creator Management:
- Add/Remove records in `tblPublicationCreator`
- CRUD operations on `tblCreator`

### Category Management:
- Add/Remove records in `tblPublicationGenre` 
- CRUD operations on `tblGenre`

### Keyword Management:
- Add/Remove records in `tblPublicationKeyWord`
- Keywords are stored directly in the relationship table (no separate master table)

### Publisher Management:
- Update `PublisherID` in `tblPublication`
- CRUD operations on `tblPublisher`

## 7. Recommended Implementation Order

1. **Start with Publisher Management** (simplest - one-to-many)
2. **Add Keyword Management** (many-to-many, but simpler since no master table)
3. **Add Author/Creator Management** (many-to-many with master table)
4. **Add Category Management** (many-to-many with master table)
5. **Create the main Publication Edit page**
6. **Integrate everything into Admin.razor**

## 8. SQL Server Considerations

Given your T-SQL background, you'll likely want to implement stored procedures for the complex operations:

```sql
-- Example stored procedure for updating publication authors
CREATE PROCEDURE sp_UpdatePublicationAuthors
    @PublicationID INT,
    @AuthorIDs NVARCHAR(MAX) -- Comma-separated list of author IDs
AS
BEGIN
    -- Delete existing relationships
    DELETE FROM tblPublicationCreator WHERE PublicationID = @PublicationID;
    
    -- Insert new relationships
    INSERT INTO tblPublicationCreator (PublicationID, CreatorID)
    SELECT @PublicationID, value
    FROM STRING_SPLIT(@AuthorIDs, ',')
    WHERE TRIM(value) != '';
END

-- Example stored procedure for updating publication categories/genres
CREATE PROCEDURE sp_UpdatePublicationGenres
    @PublicationID INT,
    @GenreIDs NVARCHAR(MAX) -- Comma-separated list of genre IDs
AS
BEGIN
    -- Delete existing relationships
    DELETE FROM tblPublicationGenre WHERE PublicationID = @PublicationID;
    
    -- Insert new relationships
    INSERT INTO tblPublicationGenre (PublicationID, GenreID)
    SELECT @PublicationID, value
    FROM STRING_SPLIT(@GenreIDs, ',')
    WHERE TRIM(value) != '';
END

-- Example stored procedure for updating publication keywords
CREATE PROCEDURE sp_UpdatePublicationKeywords
    @PublicationID INT,
    @Keywords NVARCHAR(MAX) -- JSON array of keywords
AS
BEGIN
    -- Delete existing keywords
    DELETE FROM tblPublicationKeyWord WHERE PublicationID = @PublicationID;
    
    -- Insert new keywords
    INSERT INTO tblPublicationKeyWord (PublicationID, KeyWord)
    SELECT @PublicationID, TRIM([value])
    FROM OPENJSON(@Keywords)
    WHERE TRIM([value]) != '';
END

-- Helper procedure to get all unique keywords for suggestions
CREATE PROCEDURE sp_GetAllKeywords
AS
BEGIN
    SELECT DISTINCT KeyWord
    FROM tblPublicationKeyWord
    WHERE KeyWord IS NOT NULL AND TRIM(KeyWord) != ''
    ORDER BY KeyWord;
END
```KeyWord WHERE PublicationID = @PublicationID;
    
    -- Insert new keywords
    INSERT INTO tblPublicationKeyWord (PublicationID, KeyWord)
    SELECT @PublicationID, TRIM([value])
    FROM OPENJSON(@Keywords)
    WHERE TRIM([value]) != '';
END

-- Helper procedure to get all unique keywords for suggestions
CREATE PROCEDURE sp_GetAllKeywords
AS
BEGIN
    SELECT DISTINCT KeyWord
    FROM tblPublicationKeyWord
    WHERE KeyWord IS NOT NULL AND TRIM(KeyWord) != ''
    ORDER BY KeyWord;
END
```

## Next Steps

1. Would you like me to help implement any specific component first?
2. Do you need help with the service layer implementations?
3. Would you prefer modal-based interfaces or separate pages for the management components?

The modular approach I've outlined allows you to implement and test each piece independently, making the development process more manageable.

## 10. Suggested CSS Additions

Add these styles to your existing CSS to support the keyword and chip interfaces:

```css
/* Keyword and chip styling */
.keyword-chips, .category-chips, .author-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 10px;
}

.keyword-chip, .category-chip, .author-chip {
    background: #3498db;
    color: white;
    padding: 4px 8px;
    border-radius: 12px;
    font-size: 0.9em;
    display: flex;
    align-items: center;
    gap: 5px;
}

.remove-btn {
    background: none;
    border: none;
    color: white;
    font-weight: bold;
    cursor: pointer;
    padding: 0;
    width: 16px;
    height: 16px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.remove-btn:hover {
    background: rgba(255, 255, 255, 0.2);
}

.suggestion-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    margin-top: 10px;
}

.suggestion-chip {
    background: #ecf0f1;
    color: #2c3e50;
    border: 1px solid #bdc3c7;
    padding: 6px 12px;
    border-radius: 15px;
    font-size: 0.9em;
    cursor: pointer;
    transition: all 0.2s;
}

.suggestion-chip:hover {
    background: #d5dbdb;
}

.suggestion-chip.selected {
    background: #95a5a6;
    color: white;
    cursor: not-allowed;
}

.keyword-input {
    flex: 1;
    padding: 8px 12px;
    border: 1px solid #bdc3c7;
    border-radius: 4px;
}

.large-modal .modal-content {
    max-width: 800px;
    width: 90vw;
}

.edit-sections {
    display: flex;
    flex-direction: column;
    gap: 25px;
}

.edit-section {
    background: #f8f9fa;
    padding: 20px;
    border-radius: 8px;
    border: 1px solid #e9ecef;
}

.form-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 15px;
}

.form-field.full-width {
    grid-column: 1 / -1;
}

.input-with-button {
    display: flex;
    gap: 10px;
    align-items: center;
}

.input-with-button select {
    flex: 1;
}

.no-items {
    color: #6c757d;
    font-style: italic;
}

.help-text {
    font-size: 0.9em;
    color: #6c757d;
    margin-top: 5px;
}

.more-results {
    font-size: 0.9em;
    color: #6c757d;
    margin-top: 10px;
}

.no-results {
    color: #6c757d;
    font-style: italic;
    text-align: center;
    padding: 20px;
}
```