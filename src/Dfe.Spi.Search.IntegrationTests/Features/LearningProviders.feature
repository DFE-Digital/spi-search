Feature: Searching for Learning Providers
  
  Scenario: Returning references filtered by name
    When I search for Learning Providers by name
    Then I should receive search results